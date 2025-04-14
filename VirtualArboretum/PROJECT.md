**##** Programming Practices

## Projektmanagement
 - [x] Projektplanung
 - ~~[ ] Projektstrukturierung (SIMPLES UML)~~ (! bei 1. Refactoring dann..)
 - [x] Projektinitialisierung

## Use-Cases
 0. Modelierung fertigstellen. (20%)
 1. Beliebige Dateien ein- und auslagern. (0% CLI/ 0% HTTP)
 2. Tagging von eingelagerten Dateien und deren Aggregation danach. (0% CLI/ 0% HTTP)
     - Inkludiert spezielle Tag-Typen: System-Tags/"Active" o. Automation-Tags/ 
 3. Sicherer HTTPS-Proxy (Client-Cert) über den Websites eingelagert werden können.  (0 % HTTP)
 (max. +1 weitere, wie Repository um Dateien tatsächlich zu speichern..)


## Milestones
 1.	[ ] Complete Core Domain Model:
     - Finish implementing the fundamental domain entities (Plant, Garden, Mycelium)
     - Ensure value objects like Fingerprint and Hyphae are properly integrated
 2.	[ ] Implement First Use Case End-to-End:
     - Focus on "Storing and retrieving files" as your first complete use case
     - This will validate your architecture and help identify gaps
 3.	[ ] Add Tests for Domain Logic:
     - Create unit tests for your value objects and entities
     - Test key business rules and constraints
 4.	[ ] Set Up Repository Layer:
     - Implement file-based repositories for your aggregates
     - Define clear interfaces following DIP (Dependency Inversion Principle)
 5.	[ ] Complete Basic CLI:
     - Implement basic commands for file storage and retrieval
     - Add command handling for tagging functionality
 6.	[ ] Update Project Documentation:
     - Maintain documentation of completed features
     - Update architectural diagrams as the project evolves
 7.	[ ] Implement Tagging System:
     - Once basic storage works, add the tagging functionality
     - Implement special tags like system tags


### Work packages

 - [Implemented]-[Tested]
 - [x]-[x] Hyphae attribute abstraction
 - [x]-[x] Hyphae Hierarchy to access composed Hyphae
 - [ ]-[ ] 
 - [ ]-[ ] Mycelium
 - [ ]-[ ] Plant and Plants
 - [ ]-[ ] Garden
 - [ ]-[ ] Arboretum
 - [ ]-[ ] ToolShed & Tools
 - [ ]-[STARTED] Basic CLI-Interface (~CRUD)

 - Tags einfalten und eingefaltete Tags mittels `#1`,`#2`,...,`#12345` anzeigen wenn 255 Byte overflow bevorsteht (90%).
     - Auflösen mittels right-to-left (also von hinten her aufräumen)
     - Benötigt dann jedoch call, der alle Tags ausgibt. 
     - Optimalerweise gibt es noch Priorisierung, die wichtige Tags über Dateisystem suchbar behält 
         - einmal explizit via `#tag!` und implizit nach Häufigkeit in Garten > Arboretum.
 - Aktive Tags (System-Tags `#va`) implementiert.
     - `#va-daily-growth` - Führt täglich die Wachstums-Prozedur aus (muss für Dateityp definiert werden).
     - `#va>watch>growth` - Führt Wachstums-Prozedur aus, wenn Datei geändert wurde.
     - `#va-daily-NOdecay` - Führt täglich die Verfalls-Prozedur aus (muss für Dateityp definiert werden).
     - Problem: Wie handhabst Du Prozeduren die nicht gefunden werden?
       - Einfach den tag `#va=error` oder so?
     - ``#va=track` - tracks changes in file (for versioning, should be default tho on auto-changing files)



# FAQ

 - [Use ReSharper..](https://www.jetbrains.com/help/resharper/Reference__Keyboard_Shortcuts.html#tool_windows) ([Kybrd-Shortcuts?](https://www.jetbrains.com/resharper/docs/ReSharper_DefaultKeymap_VSscheme.pdf))
 - [Code-Structure Trade-offs](https://www.jamesmichaelhickey.com/how-to-structure-your-dot-net-solutions-design-and-trade-offs/)
 