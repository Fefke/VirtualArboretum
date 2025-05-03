# Project Overview

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
 1.	✅ Complete Core Domain Model:
     - Finish implementing the fundamental domain entities..
       - ✅ Arboretum
       - ✅ Garden
       - ✅ Mycelium
       - ✅ Plant
     - Ensure value objects like Fingerprint and Hyphae are properly integrated
 2.	✅  Set Up Repository in Application Layer
     - ✅ Implement IRepositories.
     - *Define clear interfaces following DIP (Dependency Inversion Principle)*
 3.	✅ Implement First Use Case End-to-End:
     - Focus on "Storing and retrieving files" as your first complete use case
     - This will validate your architecture and help identify gaps
 4.	⭕ Add Tests for Domain Logic:
     - Create unit tests for your value objects and entities
     - Test key business rules and constraints
 5.	⭕ Complete Basic CLI:
     - Implement basic commands for file storage and retrieval
     - Add command handling for tagging functionality
 6.	⭕ Update Project Documentation:
     - Maintain documentation of completed features
     - Update architectural diagrams as the project evolves
 7. A PLantBuilder might be necessary to ensure logical association of cell to Plants.

# Ideas
 
 - `.mycelium` files for each garden represented as a directory, listing all kind of associations and connecting HyphaeStrains.


# FAQ

 - [Use ReSharper..](https://www.jetbrains.com/help/resharper/Reference__Keyboard_Shortcuts.html#tool_windows) ([Kybrd-Shortcuts?](https://www.jetbrains.com/resharper/docs/ReSharper_DefaultKeymap_VSscheme.pdf))
 - [Code-Structure Trade-offs](https://www.jamesmichaelhickey.com/how-to-structure-your-dot-net-solutions-design-and-trade-offs/)
 
 - *Bei DTO-Instanziierung*: Unbedingt field names verwenden, sonst hast Du vielleicht noch ein dummes Ordinalitäts-Problem.