# Rejestr urządzeń sieciowych

Konsolowa aplikacja CRUD w C# do ewidencji routerów, przełączników, punktów dostępowych i serwerów.
Projekt z przedmiotu „Programowanie obiektowe”. Dane są zapisywane w pliku JSON i zachowują się między uruchomieniami.

Pełna instrukcja obsługi ze zrzutami ekranu: [`dokumentacja/dokumentacja.pdf`](dokumentacja/dokumentacja.pdf).

## Uruchomienie

Wymagany jest .NET 9 SDK.

```
dotnet run --project src/RejestrUrzadzen
```

Inny plik danych można wskazać argumentem:

```
dotnet run --project src/RejestrUrzadzen -- moj_rejestr.json
```

## Struktura repozytorium

- `src/RejestrUrzadzen/` – kod programu (`Domain`, `Persistence`, `Services`, `Ui`)
- `dokumentacja/` – źródła LaTeX i PDF dokumentacji
- `dokumentacja/narzedzia/` – skrypty do zrzutów ekranu (`zrzuty.py`, opis w `ZRZUTY.md`) i kompilacji dokumentacji (`kompiluj.sh`)
