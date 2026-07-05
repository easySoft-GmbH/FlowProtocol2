# FlowProtocol 2 Skriptsprache

Diese Datei beschreibt die FlowProtocol-2-Skriptsprache in einer kompakten Form. Sie dient als Grundlage für die KI-gestützte Erzeugung neuer FlowProtocol-2-Skripte und orientiert sich an den Beispielskripten in `Scripts/FP2-Tutorial` und `Scripts/Doku-Beispiele` sowie an den Befehlsimplementierungen im Ordner `FlowProtocol2/Commands`.

## Allgemeines

FlowProtocol-2-Skripte sind normale Textdateien mit der Endung `.fp2`.
- Leerzeilen und Zeilen, die nur aus Whitespace bestehen, werden ignoriert.
- Kommentarzeilen beginnen mit `//` und werden nicht ausgeführt.
- Zeilen, die mit `__` beginnen, werden als Fortsetzung der vorherigen Zeile behandelt.

Beispiel:
```text
// Kommentar wird ignoriert
@Ausgabe >> Diese Zeile wird
    __ auf mehrere Zeilen
    __ umgebrochen.
```

## Syntax-Grundlagen

### Befehle
FlowProtocol-2-Befehle beginnen mit `~`.
Beispiele:
- `~Set Name=Max`
- `~If $Wert==1`
- `~Input Key: Frage`

### Ausgabezeilen
Ausgabezeilen beginnen mit `@` und/oder `>`.
Die Syntax ist flexibel und unterstützt mehrere Ausgabeebenen und Formattypen.

Beispiele:
- `@Ausgabe >> Hallo $Name`
- `> * Listenpunkt`
- `>>| Codezeile`
- `@Section > Überschrift`

### Einrückung
Die Einrückung steuert zusammengehörige Blöcke.
- Ein `~If`-Block umfasst die nachfolgenden eingerückten Zeilen bis zur gleichen oder geringeren Einrückung.
- `~Loop` beendet Schleifenblöcke wie `~DoWhile`, `~ForEach`, `~For` oder `~ForEachLine`.

## Variablen und Ausdrucksersetzung

Variablen werden mit `$` referenziert. Beispiel: `$Name`.

### Interne Variablen
- `$BaseKey` – aktueller Basis-Schlüssel, der von `~Include` und Eingabebefehlen verwendet wird.
- `$NewGuid` – erzeugt eine neue GUID.
- `$CRLF` – Wagenrücklauf + Zeilenvorschub.
- `$LF` – Zeilenvorschub.
- `$ScriptFilePath`, `$ScriptPath`, `$CurrentScriptPath` – Pfadinformationen.
- `$BaseURL`, `$ResultURL` – URL-Informationen für die Webausführung.
- `$LineNumber` und `$LineNumber-n` – aktuelle Skriptzeilennummer.
- `$Chr(n)` – ersetzt durch das Zeichen mit dem ASCII-Code `n`.

Die Ersetzung erfolgt rekursiv, soweit sich die Zeichenketten verändern. Es gibt ein internes Limit für die maximale Ersetzungsgröße.

## Daten- und Textoperationen

### `~Set`
Weist einer Variablen einen Wert zu.
```text
~Set Ergebnis=42
~Set Text=Hallo $Name
```

### `~Replace`
Ersetzt Text innerhalb eines Strings.
```text
~Replace Text=$Text|alt->neu
```

### `~Split`
Teilt einen String anhand eines Trennzeichens in eine Liste.
```text
~Split Liste=$Text|;
```

### `~Trim`, `~ToUpper`, `~ToLower`, `~CamelCase`
Textformate und -transformationen.

### `~UrlEncode`, `~XmlEncode`
Kodiert Zeichenketten für URL bzw. XML.

### Reguläre Ausdrücke
- `~RegExReplace` ersetzt Texte per Regex.
- `~RegExMatch` extrahiert Treffer aus einem Ausdruck.

### Berechnungen
- `~Calculate` führt einfache arithmetische Operationen aus.
- `~CalculateExpression` wertet komplexere Ausdrücke aus.
- `~Round` rundet Zahlen.
- `~Random` erzeugt Zufallswerte.
- `~Sort` sortiert Listen.

## Datums- und Zeitbefehle

### `~SetDateTime`
Setzt eine Variable auf ein Datum/Uhrzeit.

### `~DateSet`
Interpretiert einen Text als Datum.

### `~DateAdd`
Addiert eine Zeitspanne zum Datum.

### `~DateDiff`
Berechnet die Differenz zwischen zwei Zeitpunkten.

### `~DateFormat`
Formatiert ein Datum im gewünschten Schema.

### `~SetCulture`
Setzt die Kultur, die sich auf Datums- und Zahlenformate auswirkt.

## Steuerung und Programmablauf

### Bedingungen
- `~If <Bedingung>`
- `~ElseIf <Bedingung>`
- `~Else`

Bedingungen unterstützen:
- `==`, `!=`, `<>`, `<`, `>`, `<=`, `>=`
- `~` (enthält)
- `!~` (enthält nicht)
- `&&`, `||`
- Sonderfälle: `1`, `0`, `true`, `false`

### Schleifen
- `~DoWhile <Bedingung>`
- `~ForEach <Variable> in <Feld>`
- `~For <Variable> in <Von>..<Bis>[; Step=<Schritt>]`
- `~ForEachLine <Variable> in <Datei> [; Take=<Anzahl>][; IndexVar=<Index>][; SectionVar=<Abschnitt>][; NoFormat]`
- `~Loop`
- `~ExitLoop`

### Funktionen und Sprünge
- `~DefineSub <Name>` definiert einen Funktionsblock.
- `~GoSub <Name>` springt in eine Funktion.
- `~Return` kehrt aus einer Funktion zurück.
- `~GoTo <Marke>` springt zu einer Sprungmarke.
- `~JumpMark <Name>` definiert eine Sprungmarke.

### Sektion und Ablauf
- `~Include <Datei>.fps [; BaseKey=<Basisschlüssel>]` lädt ein externes Funktionsskript.
- `~SetTitle <Titel>` setzt einen Titel.
- `~SetSection <Abschnitt>` setzt die aktuelle Ausgabesektion.
- `~SetBlockSaveFile <Datei>` steuert die Blockspeicherung.
- `~SetStopCounter <Anzahl>` begrenzt die Ausführungsschritte.
- `~End` beendet die Ausführung sofort.
- `~EndParagraph` beendet den aktuellen Absatz in der Ausgabe.

## Eingabe und Interaktion

### Texteingabe
- `~Input <Schlüssel>:<Frage>` fragt einen einzeiligen Text ab.

### Multiline-Eingabe
- `~MultiLineInput <Key>: <Prompt>[; ShowLines=<Anzahl>][; UploadFilter=<Filter>][; ReadRegEx=<Regex>]`

Optionen:
- `ShowLines` legt die sichtbare Höhe des Eingabefeldes fest.
- `UploadFilter` aktiviert die Auswahl von Dateien für den Import.
- `ReadRegEx` filtert beim Einlesen einer Datei nach Regex-Treffern.

### Auswahlgruppen
- `?Key:Prompt` erzeugt eine Auswahlgruppe.
- `#Wert:Anzeige` definiert eine Option innerhalb einer Gruppe.
- `~DynamicOptionGroup` erzeugt optionale Werte dynamisch aus einer Liste.

### Hilfeelemente
- `~AddHelpLine <Text>` fügt eine Hilfszeile unterhalb der letzten Eingabe ein.
- `~AddHelpLink <URL> | <Anzeige>` fügt einen Hilfslink ein.
- `~AddHelpText <Text>` ergänzt erklärenden Hilfstext.

### Ausführung von Eingaben
- `~Execute` sorgt dafür, dass alle zuvor stehenden Eingabebefehle ausgeführt werden, wenn sie noch keinen Wert besitzen.

## Dateioperationen und Listen

### Prüfen und Auflisten
- `~FileExists <Pfad>` prüft, ob eine Datei existiert.
- `~ListFiles <Pfad>` listet Dateien auf.
- `~ListDirectories <Pfad>` listet Ordner auf.

### Werte hinzufügen und verwalten
- `~AddTo <Variable>=<Wert>` addiert einen numerischen Wert zu einer Variablen.
- `~AddToList <Liste>=$Wert` fügt einen Wert ans Ende einer Liste hinzu.
- `~AddNewKey <Variable>=<Wert>` fügt einen Wert zur URL hinzu, falls er noch nicht vorhanden ist.
- `~ClearVar <Muster>` löscht Variablen, die auf ein Muster passen.

## Dokumentenausgabe

Die Ausgaben in FlowProtocol 2 werden als strukturierte Textblöcke erzeugt.
Die Syntax `@...` und `>` steuert Überschriften, Listen, Absätze und Codeblöcke.

Beispiel:
```text
@Kapitel > Einführung
> * Erster Punkt
>>* Unterpunkt
>| Codezeile
```

## Beispiele

### Einfache Ausgabe und Variable
```text
~Set Name=Anna
@Ausgabe >> Hallo $Name!
```

### Bedingung und Schleife
```text
~Set Count=0
~DoWhile $Count<3
    ~Set Count=$Count+1
    @Ausgabe >> Schleifenlauf $Count
~Loop
```

### Eingabe und Ausgabe
```text
~Input Name: Wie heißt du?
~Execute
@Ausgabe >> Willkommen, $Name!
```

### Dateizeilen verarbeiten
```text
~ForEachLine Zeile in Quelldaten.txt; IndexVar=idx
    @Ausgabe >> Zeile $idx: $Zeile
~Loop
```

## Hinweise für die KI-Generierung

- Skripte sollten mit klaren `~`-Befehlen und sichtbaren Einrückungen geschrieben werden.
- Eingabeschlüssel dürfen keine Leerzeichen enthalten.
- Für Wiederverwendung und parametrische Eingaben empfiehlt sich `~Include` mit `BaseKey`.
- Ausgaben werden am besten über `@`/`>`-Zeilen strukturiert, nicht als reine Textzeilen.
- Bedingungen sollten so einfach wie möglich formuliert werden und Variablen mit `$` verwenden.

## Relevante Befehle

Die wichtigsten FlowProtocol-2-Befehle umfassen:
- Eingabe: `~Input`, `~MultiLineInput`, `?`, `#`, `~Execute`, `~AddHelpLine`, `~AddHelpLink`, `~AddHelpText`
- Text und Ausgabe: `@`, `~AddText`, `~AddLink`, `~AddCode`, `~EndParagraph`
- Steuerung: `~If`, `~ElseIf`, `~Else`, `~DoWhile`, `~ForEach`, `~For`, `~Loop`, `~ExitLoop`, `~GoSub`, `~Return`, `~GoTo`, `~JumpMark`, `~DefineSub`, `~Include`, `~End`
- Variablen und Daten: `~Set`, `~Replace`, `~Split`, `~Trim`, `~ToUpper`, `~ToLower`, `~CamelCase`, `~UrlEncode`, `~XmlEncode`, `~RegExReplace`, `~RegExMatch`, `~Calculate`, `~CalculateExpression`, `~Round`, `~Random`, `~Sort`, `~AddTo`, `~AddToList`, `~AddNewKey`, `~ClearVar`
- Datum/Uhrzeit: `~SetDateTime`, `~DateSet`, `~DateAdd`, `~DateDiff`, `~DateFormat`, `~SetCulture`
- Dateisystem: `~FileExists`, `~ListFiles`, `~ListDirectories`, `~ForEachLine`
- Struktur: `~SetTitle`, `~SetSection`, `~SetBlockSaveFile`, `~SetStopCounter`, `~Implies`, `~DynamicOptionGroup`

## Abschluss

FlowProtocol-2-Skripte sind bewusst einfach und auf lesbare Textstruktur ausgelegt. Sie kombinieren Eingabe-/Ausgabe-Elemente, Variablenersetzung und steuernde Programmstrukturen in einer Zeilenorientierung, die sich gut für browserbasierte Ausführung und interaktive Hilfestellungen eignet.
