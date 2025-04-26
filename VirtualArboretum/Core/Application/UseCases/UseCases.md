# UseCases

Use Cases orchestrate any business logic workflows:
 - It loads up required Entities (z. B. Plant, Garden, Arboretum) via **Repository-Interfaces**.
 - It does run **core domain-logic** (either in Use Case or by calling Methods from domain-entities).
 - It does use Repository-Interfaces, to store any changes made to entities.
 - It does prepare DataTransferObjects (**DTO**) and presents them to the calling layer,
   often via an **Output Port Interface**, being implemented by a Presenter.
 - It **does catch any internal Exceptions** and reformulates them inside a Error DTO.