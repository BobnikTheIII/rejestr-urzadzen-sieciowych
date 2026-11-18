#!/usr/bin/env python3
"""Wykonuje scenariusz na prawdziwym programie i zapisuje zrzuty ekranu do dokumentacji.

Program działa w pseudoterminalu (100x50). Stan ekranu w chwili zrzutu jest zamieniany na HTML
w wyglądzie Konsole z domyślnym schematem kolorów Breeze (Nobara Linux) i fotografowany
przez Chrome w trybie headless. Nic nie jest wpisywane ręcznie do obrazków: zawartość
każdego zrzutu pochodzi z wyjścia programu. Szczegóły: ZRZUTY.md w tym katalogu.

Użycie: python3 zrzuty.py <RejestrUrzadzen.dll> <katalog_wyjściowy>
"""
import fcntl
import html
import os
import pty
import re
import select
import shutil
import struct
import subprocess
import sys
import tempfile
import termios
import time
from pathlib import Path

import numpy
from PIL import Image, ImageChops

COLS, ROWS = 100, 50
CLEAR = re.compile(rb"\x1b\[H\x1b\[2J(?:\x1b\[3J)?|\x1b\[3J\x1b\[H\x1b\[2J")
NON_SGR = re.compile(rb"\x1b\[\??[0-9;]*[A-Za-ln-z]|\x1b[=>]|\x1b\(B")


class Session:
    """Uruchomiony proces programu podłączony do pseudoterminala."""

    def __init__(self, dll, data_file, workdir):
        self.buffer = b""
        self.pid, self.fd = pty.fork()
        if self.pid == 0:
            os.chdir(workdir)
            os.environ["TERM"] = "xterm-256color"
            os.environ["DOTNET_NOLOGO"] = "1"
            os.execvp("dotnet", ["dotnet", dll, data_file])
        fcntl.ioctl(self.fd, termios.TIOCSWINSZ, struct.pack("HHHH", ROWS, COLS, 0, 0))
        self.settle(first=True)

    def settle(self, first=False):
        """Czyta wyjście, aż program przestanie pisać (czeka wtedy na dane)."""
        deadline = time.time() + 15
        idle = 1.0 if first else 0.25
        got_any = False
        while time.time() < deadline:
            ready, _, _ = select.select([self.fd], [], [], idle)
            if not ready:
                if got_any or not first:
                    return
                continue
            try:
                chunk = os.read(self.fd, 65536)
            except OSError:
                return
            if not chunk:
                return
            self.buffer += chunk
            got_any = True

    def send(self, *lines):
        for line in lines:
            os.write(self.fd, line.encode() + b"\n")
            self.settle()
        return self

    def screen(self):
        """Tekst widoczny od ostatniego wyczyszczenia ekranu."""
        last = CLEAR.split(self.buffer)[-1]
        # Spacja po znaku zachęty zostaje - za nią stoi kursor.
        return NON_SGR.sub(b"", last).replace(b"\r\n", b"\n").rstrip(b"\n")

    def close(self):
        os.close(self.fd)
        os.waitpid(self.pid, 0)


class KonsoleStyle:
    """Wygląd Konsole: schemat kolorów Breeze i czcionka Noto Sans Mono.

    Wartości zmierzono na zrzutach z Konsole w Nobara Linux:
    tło #232627, szerokość znaku ok. 10,3 px, odstęp wierszy ok. 23,4 px.
    """

    BACKGROUND = (35, 38, 39)
    FOREGROUND = (252, 252, 252)
    # Kolory 0-7 (kody SGR 30-37) i ich jasne odpowiedniki 8-15 (kody 90-97).
    NORMAL = [(35, 38, 39), (237, 21, 21), (17, 209, 22), (246, 116, 0),
              (29, 153, 243), (155, 89, 182), (26, 188, 156), (252, 252, 252)]
    INTENSE = [(127, 140, 141), (192, 57, 43), (28, 220, 154), (253, 188, 75),
               (61, 174, 233), (142, 68, 173), (22, 160, 133), (255, 255, 255)]
    FONT = "'Noto Sans Mono', monospace"
    FONT_SIZE = 17.2  # px; szerokość znaku = 0,6 em = 10,3 px
    LINE_HEIGHT = 23.4  # px
    PADDING = 5  # px od krawędzi okna do tekstu

    @staticmethod
    def css(rgb):
        return "#%02x%02x%02x" % rgb

    def color_for(self, code):
        if 30 <= code <= 37:
            return self.NORMAL[code - 30]
        if 90 <= code <= 97:
            return self.INTENSE[code - 90]
        return None


SGR = re.compile(r"\x1b\[([0-9;]*)m")


class Renderer:
    """Zamienia tekst z kodami ANSI na obraz PNG wyglądający jak okno Konsole."""

    def __init__(self, output, scratch, style=None):
        self.output = Path(output)
        self.scratch = Path(scratch)
        self.style = style or KonsoleStyle()
        self.output.mkdir(parents=True, exist_ok=True)

    def to_html(self, text, cursor):
        """Kody SGR -> elementy <span> z kolorami ze schematu; na końcu opcjonalny kursor."""
        parts, color, position = [], None, 0
        source = text.decode("utf-8", errors="replace")
        for match in SGR.finditer(source):
            parts.append(self.span(source[position:match.start()], color))
            for code in (int(c) for c in (match.group(1) or "0").split(";") if c):
                if code in (0, 39):
                    color = None
                elif self.style.color_for(code):
                    color = self.style.color_for(code)
            position = match.end()
        parts.append(self.span(source[position:], color))
        if cursor:
            parts.append('<span class="cursor"> </span>')
        return "".join(parts)

    def span(self, text, color):
        escaped = html.escape(text)
        if not escaped or color is None:
            return escaped
        return f'<span style="color:{self.style.css(color)}">{escaped}</span>'

    def shot(self, name, text, cursor=True):
        """cursor=False dla ekranów, po których program już się zakończył."""
        st = self.style
        body = self.to_html(text, cursor)
        lines = text.count(b"\n") + 1
        page = self.scratch / f"{name}.html"
        page.write_text(
            "<!doctype html><meta charset=utf-8><style>"
            f"html,body{{margin:0;background:{st.css(st.BACKGROUND)}}}"
            f"pre{{margin:0;padding:{st.PADDING}px;width:{COLS}ch;color:{st.css(st.FOREGROUND)};"
            f"font-family:{st.FONT};font-size:{st.FONT_SIZE}px;line-height:{st.LINE_HEIGHT}px;"
            "white-space:pre-wrap;overflow-wrap:anywhere;font-kerning:none;font-variant-ligatures:none}"
            f".cursor{{display:inline-block;width:1ch;height:{st.LINE_HEIGHT - 2}px;vertical-align:-4px;"
            f"background:{st.css(st.FOREGROUND)}}}</style>"
            f"<pre>{body}</pre>", encoding="utf-8")
        width = round(COLS * st.FONT_SIZE * 0.6 + 2 * st.PADDING + 2)
        height = round((lines + 6) * st.LINE_HEIGHT + 2 * st.PADDING)
        target = self.output / f"{name}.png"
        subprocess.run(
            ["google-chrome", "--headless=new", "--no-sandbox", "--disable-gpu", "--hide-scrollbars",
             f"--user-data-dir={self.scratch / 'chrome'}", "--force-device-scale-factor=2",
             f"--window-size={width},{height}", f"--screenshot={target}", f"file://{page}"],
            check=True, capture_output=True)
        self.trim_bottom(target)
        print(f"{target.name}: {lines} wierszy")

    def trim_bottom(self, path):
        """Obcina pusty dół obrazu, zostawiając margines. Szerokość się nie zmienia,
        dzięki czemu wszystkie zrzuty mają w dokumentacji tę samą skalę."""
        image = Image.open(path).convert("RGB")
        background = Image.new("RGB", image.size, self.style.BACKGROUND)
        bottom = ImageChops.difference(image, background).getbbox()[3]
        margin = round(self.style.LINE_HEIGHT * 2)
        image = image.crop((0, 0, image.width, min(image.height, bottom + margin)))
        self.save_compressed(image, path)

    def save_compressed(self, image, path, colors=256):
        """Zapisuje PNG z paletą kolorów zamiast pełnego RGB, co zmniejsza plik kilka razy.

        Większość palety dobiera automatycznie Pillow (median cut), ale samo to przesuwało
        kolory tekstu o kilka odcieni. Dlatego tło i wszystkie kolory schematu Breeze, które
        występują na obrazie, są dopisywane do palety dokładnie, a piksele dostają
        najbliższy kolor z tej palety bez ditheringu."""
        style = self.style
        used = set(c for _, c in image.getcolors(1 << 24))
        exact = [c for c in dict.fromkeys([style.BACKGROUND, style.FOREGROUND,
                                           *style.NORMAL, *style.INTENSE]) if c in used]
        adaptive = image.quantize(colors=colors - len(exact), dither=Image.Dither.NONE)
        raw = adaptive.getpalette()[:3 * len(adaptive.getcolors())]
        palette = exact + [tuple(raw[k:k + 3]) for k in range(0, len(raw), 3)]
        palette = list(dict.fromkeys(palette))[:colors]
        # Mapowanie na paletę liczone ręcznie: Image.quantize(palette=...) zaokrągla
        # kolory i zamieniało np. czerwony (192, 57, 43) na (192, 56, 41).
        pixels = numpy.asarray(image, dtype=numpy.int32).reshape(-1, 3)
        unique, inverse = numpy.unique(pixels, axis=0, return_inverse=True)
        table = numpy.array(palette, dtype=numpy.int32)
        nearest = ((unique[:, None, :] - table[None, :, :]) ** 2).sum(axis=2).argmin(axis=1)
        indexed = nearest[inverse.reshape(-1)].astype(numpy.uint8).reshape(image.height, image.width)
        result = Image.fromarray(indexed, mode="P")
        result.putpalette([v for c in palette for v in c])
        result.save(path, optimize=True)


ROUTER = ["1", "rtr-brzeg-01", "192.168.0.1", "Serwerownia A", "1", "2", "t", "Główny router brzegowy"]
SWITCH = ["2", "sw-rdzen-01", "192.168.0.2", "Serwerownia A", "1", "48", "t", ""]
ACCESS_POINT = ["3", "ap-parter-01", "192.168.10.11", "Parter", "1", "Kampus-Goście", "4", ""]
SERVER = ["4", "srv-pliki-01", "192.168.0.20", "Serwerownia B", "1", "16", "64", "Debian 13", "Serwer plików"]
SPARE_SWITCH = ["2", "sw-dostep-02", "192.168.0.3", "Magazyn IT", "2", "24", "n", "Sprzęt zapasowy"]


def add(session, fields):
    session.send("1", *fields)
    return session


def main():
    dll, output = os.path.abspath(sys.argv[1]), sys.argv[2]
    work = Path(tempfile.mkdtemp())
    render = Renderer(output, work)
    data = "urzadzenia.json"

    s = Session(dll, data, work)
    render.shot("menu", s.screen())

    s.send("9")
    render.shot("menu_blad", s.screen())
    s.send("")

    s.send("2")
    render.shot("lista_pusta", s.screen())
    s.send("")

    add(s, ROUTER)
    render.shot("dodaj_router", s.screen())
    s.send("")
    add(s, SWITCH)
    render.shot("dodaj_przelacznik", s.screen())
    s.send("")
    add(s, ACCESS_POINT)
    render.shot("dodaj_ap", s.screen())
    s.send("")
    add(s, SERVER)
    render.shot("dodaj_serwer", s.screen())
    s.send("")
    add(s, SPARE_SWITCH)
    s.send("")

    # Niepoprawne dane: każde pole jest powtarzane, aż wartość będzie poprawna.
    s.send("1", "7", "1", "", "rtr-brzeg-01", "rtr-zapas-01", "192.168.1", "192.168.0.1", "192.168.0.9",
           "Magazyn IT", "8", "2", "abc", "0", "1", "moze", "n")
    s.send("!")
    render.shot("dodaj_anuluj", s.screen())
    s.send("")

    s.send("2")
    render.shot("lista", s.screen())
    s.send("")

    s.send("3", "abc", "99", "3")
    render.shot("szczegoly", s.screen())
    s.send("")

    s.send("4", "Serwerownia A")
    render.shot("szukaj_lokalizacja", s.screen())
    s.send("")
    s.send("4", "debian")
    render.shot("szukaj_pole_typu", s.screen())
    s.send("")
    s.send("4", "drukarka")
    render.shot("szukaj_brak", s.screen())
    s.send("")

    s.send("5", "3", "", "", "Pierwsze piętro", "3", "Kampus-Pracownicy", "", "Po przeniesieniu")
    render.shot("edycja", s.screen())
    s.send("")
    s.send("3", "3")
    render.shot("edycja_wynik", s.screen())
    s.send("")
    s.send("5", "3", "sw-rdzen-01", "!")
    render.shot("edycja_konflikt", s.screen())
    s.send("")

    s.send("6", "5", "n")
    render.shot("usun_rezygnacja", s.screen())
    s.send("")
    s.send("6", "5", "t")
    render.shot("usun", s.screen())
    s.send("")

    s.send("7")
    render.shot("podsumowanie", s.screen())
    s.send("")

    s.send("0")
    render.shot("wyjscie", s.screen(), cursor=False)
    s.close()

    # Drugie uruchomienie: dane muszą pochodzić z pliku.
    shutil.copy(work / data, Path(output) / "urzadzenia_przyklad.json")
    s = Session(dll, data, work)
    s.send("2")
    render.shot("trwalosc", s.screen())
    s.send("", "1", "1", "rtr-nowy-01", "10.0.0.1", "Parter", "1", "1", "n", "", "", "0")
    s.close()
    s = Session(dll, data, work)
    s.send("2")
    render.shot("trwalosc_id", s.screen())
    s.send("", "0")
    s.close()

    # Uszkodzony plik danych: program odmawia pracy i nie nadpisuje pliku.
    broken = work / "uszkodzony.json"
    broken.write_text('{"nextId": 2, "devices": [', encoding="utf-8")
    before = broken.read_bytes()
    s = Session(dll, "uszkodzony.json", work)
    render.shot("plik_uszkodzony", s.screen(), cursor=False)
    s.close()
    assert broken.read_bytes() == before, "program zmienił uszkodzony plik"

    shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    main()
