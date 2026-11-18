# Zrzuty ekranu konsoli w dokumentacji

Ten plik opisuje, jak powstają zrzuty ekranu programu w `dokumentacja/figures/` i jak odwzorować
w nich wygląd konsoli Konsole z systemu Nobara Linux.

## Skąd się biorą zrzuty

Zrzuty nie są fotografiami okna. Robi je skrypt `zrzuty.py`, który:

1. uruchamia skompilowany program (`RejestrUrzadzen.dll`) w pseudoterminalu o rozmiarze
   100 kolumn × 50 wierszy (`TERM=xterm-256color`),
2. wpisuje do programu kolejne dane według scenariusza zapisanego w funkcji `main()`,
3. w chwili zrzutu bierze wszystko, co program wypisał od ostatniego wyczyszczenia ekranu
   (`Console.Clear()`), czyli dokładnie to, co widać w terminalu,
4. zamienia kolory ANSI na HTML w wyglądzie Konsole (klasa `KonsoleStyle`),
5. fotografuje tę stronę przeglądarką Google Chrome w trybie headless w podwójnej
   rozdzielczości (`--force-device-scale-factor=2`), żeby obrazy były ostre w PDF-ie,
6. obcina pusty dół obrazu. Szerokość wszystkich zrzutów zostaje taka sama (100 kolumn),
   więc w dokumentacji mają jednakową skalę tekstu.
7. zapisuje PNG z paletą 256 kolorów zamiast pełnego RGB (`Renderer.save_compressed`), przez co
   pliki są ok. 2,5 raza mniejsze. Tło i kolory Breeze użyte na obrazie są w palecie dokładnie,
   resztę palety (wygładzone krawędzie liter) dobiera Pillow. Piksele są przypisywane do
   najbliższego koloru palety w numpy, bo `Image.quantize(palette=...)` zaokrągla kolory
   i lekko przesuwało np. czerwony i turkusowy.

Treść każdego zrzutu pochodzi z prawdziwego wyjścia programu: nic nie jest dopisywane ręcznie.
Skrypt sprawdza też, że program nie nadpisał uszkodzonego pliku danych.

## Wzorzec wyglądu

Wzorcem były zrzuty z Konsole w Nobara Linux (`obrazyDoPodstawienia/` w katalogu projektu,
poza repozytorium). Ich wygląd odpowiada **domyślnemu, wbudowanemu schematowi kolorów Breeze**
w Konsole i czcionce **Noto Sans Mono** (domyślna czcionka o stałej szerokości w Nobara).

Uwaga: w `/usr/share/konsole/` jest też schemat `Nobara.colorscheme`, ale wzorcowe zrzuty go nie
używają. Ma on tło `#151B21` i inne kolory, a we wzorcu tło to `#232627`, czyli Breeze.

### Kolory

Program używa czterech kolorów konsoli .NET. Na Linuksie .NET wysyła je jako jasne kolory ANSI
(kody 90–97), które Konsole rysuje kolorami „Intense” ze schematu:

| Element w programie                  | `ConsoleColor` | Kod SGR | Kolor (Breeze) | RGB           |
|--------------------------------------|----------------|---------|----------------|---------------|
| Tło okna                             | –              | –       | `#232627`      | 35, 38, 39    |
| Zwykły tekst                         | domyślny       | `39`    | `#FCFCFC`      | 252, 252, 252 |
| Tytuły, nagłówki tabel               | `Cyan`         | `96`    | `#16A085`      | 22, 160, 133  |
| Komunikaty błędów                    | `Red`          | `91`    | `#C0392B`      | 192, 57, 43   |
| Podpowiedzi, ścieżka pliku danych    | `DarkGray`     | `90`    | `#7F8C8D`      | 127, 140, 141 |
| Komunikaty o powodzeniu              | `Green`        | `92`    | `#1CDC9A`      | 28, 220, 154  |
| Kursor (prostokąt)                   | –              | –       | `#FCFCFC`      | 252, 252, 252 |

Pełna paleta (kody 30–37 i 90–97) jest w klasie `KonsoleStyle` w `zrzuty.py`. Kolory
zweryfikowano, odczytując najbardziej nasycone piksele liter na zrzutach wzorcowych.

### Czcionka i odstępy

Zmierzone na wzorcu (zrzuty w skali 1:1):

| Parametr                         | Wartość                      | W `KonsoleStyle` |
|----------------------------------|------------------------------|------------------|
| Czcionka                         | Noto Sans Mono, zwykła       | `FONT`           |
| Szerokość znaku                  | ok. 10,3 px                  | `FONT_SIZE = 17.2` (0,6 em) |
| Odstęp między wierszami          | ok. 23,4 px                  | `LINE_HEIGHT = 23.4` |
| Odstęp tekstu od krawędzi okna   | ok. 5 px                     | `PADDING = 5`    |

Kursor jest pełnym prostokątem w kolorze tekstu, wstawionym tam, gdzie program czeka na dane.
Na ekranach, po których program się zakończył (wyjście, uszkodzony plik), kursora nie ma.

## Jak wygenerować zrzuty od nowa

Potrzebne są: .NET 9 SDK, Python 3 z bibliotekami Pillow i numpy, Google Chrome i czcionka Noto Sans Mono
(w Nobara jest domyślnie). Z głównego katalogu repozytorium:

```
dotnet build RejestrUrzadzen.sln -c Release
python3 dokumentacja/narzedzia/zrzuty.py src/RejestrUrzadzen/bin/Release/net9.0/RejestrUrzadzen.dll dokumentacja/figures
cd dokumentacja && narzedzia/kompiluj.sh
```

Skrypt nadpisuje pliki PNG w `dokumentacja/figures/` i plik `urzadzenia_przyklad.json`.
Nazwy plików odpowiadają tym w `R3.tex` (makro `\zrzut{nazwa}{podpis}`). Po zmianie
komunikatów programu trzeba sprawdzić, czy tekst w rozdziale 3 nadal zgadza się z obrazkami.

Gdy zmienia się scenariusz (inne dane, nowy zrzut), edytuje się funkcję `main()`:
`s.send(...)` wpisuje kolejne wiersze, a `render.shot("nazwa", s.screen())` zapisuje obraz.

## Jak dopasować wygląd do innego wzorca

1. Zrób zrzut prawdziwego okna terminala w skali 1:1, np. w Spectacle: tryb „Prostokątny
   obszar” (`Meta+Shift+Print`) lub tylko aktywne okno: `spectacle -a -b -n -o wzorzec.png`.
2. Odczytaj kolory. Kolor tła to najczęstszy piksel, a kolor tekstu to najbardziej nasycony
   piksel w danym wierszu (krawędzie liter są wygładzane, więc są ciemniejsze):

   ```python
   from PIL import Image
   im = Image.open("wzorzec.png").convert("RGB")
   tlo = max(im.getcolors(1 << 24))[1]
   wiersz = [im.getpixel((x, y)) for y in range(Y0, Y1) for x in range(im.width)]
   print(tlo, max(wiersz, key=lambda p: p[1] - sum(p) / 3))  # p[0]: czerwony, p[1]: zielony
   ```

   Kolory schematu można też przepisać wprost z pliku `.colorscheme` z `/usr/share/konsole/`
   lub `~/.local/share/konsole/` (sekcje `Color0`…`Color7` i `…Intense`).
3. Zmierz szerokość znaku: (prawy koniec długiego wiersza − lewy koniec) ÷ liczba znaków,
   a odstęp wierszy jako różnicę położenia kolejnych linii tekstu.
   `FONT_SIZE` = szerokość znaku ÷ 0,6 (dla czcionek o proporcjach Noto Sans Mono).
4. Wpisz wartości do `KonsoleStyle`, wygeneruj zrzuty i porównaj je ze wzorcem obok siebie,
   np. `magick wzorzec.png \( figures/menu.png -resize 50% \) -append porownanie.png`
   (zrzuty są w podwójnej rozdzielczości, stąd `-resize 50%`).

## Ograniczenia

- Obsługiwane są tylko kolory tekstu (kody SGR 30–37, 90–97, 0 i 39), bo program nie używa
  pogrubień ani kolorowego tła.
- Chrome wygładza czcionki trochę inaczej niż Konsole, więc litery mogą się minimalnie różnić
  grubością. Na wydruku tej różnicy nie widać.
- Wiersze dłuższe niż 100 znaków są zawijane tak jak w terminalu, ale bez ramki ani paska
  tytułu okna: zrzuty pokazują sam obszar terminala, tak jak wzorzec.
