# README - vArboretum

<img src="./Infrastructure/StaticResources/VirtualArboretumLogo.png" alt="Alternativer Text" width="240">

Your digital garden organization tool.
Let everything that fits in files grow and flourish in your virtual arboretums decentralized gardens.  
Access it via attribute-based queries and grow your knowledge trees.


Lagere Herbarien zum Ansehen und lernen  
oder lebendige Pflanzen zum inkubieren und wachsen ein.

## Glossar

Arboretum - Ist die Gesamtheit der Anwendung und enthält die Unterkomponenten der Gardens, Mycelium, 

Garden - Ist das Aggregat mehrere Pflanzen und wird in der Realität als ein Verzeichnis repräsentiert, das weitere Gartenabschnitte (ebenfalls Garden) aufweisen kann.

Plant - repräsentiert eine Datei, die mit Metadaten, wie einem Namen, einem Format,  ausgestattet ist und von Hyphae mit anderen Pflanzen und der Bedeutung der konkreten Hypha assoziiert wird.

Mycelium - ist die Aggregation aller Hyphae (Verbindungen/Kanten) die den Graphen abbilden, der durch alle Attribuierungen implizit erstellt wird.

Hyphae - sind Verbindungselemente, die verschiedene Pflanzen innerhalb meines Mycels verbinden. Eine reflektive Hypha wird MarkerHypha genannt und ist mit einem Tag gleichzusetzen, da die Hypha nur für sich selbst steht und dennoch mit vielen Plants assoziiert werden kann.



## How to use..

```
docker exec -it container-id va #...
# or
docker run -it dein-image-name va #...
```
### Use-Cases

 - Einlagern von Pflanzen.
 - Betrachten von Pflanzen.
 - Pflanzen wachsen lassen (sprich eine neue Version an HEAD setzen).
 - Pflanzen über Hyphae in Mycelium mit bestimmten Verbindungen und Eigenschaften assoziieren.
 - Pflanzen zu gegebenen Hyphae auslesen (mit AND, OR, NOT Logik).
 - Pflanzen, wie Hyphae vernichten. 


### Hybrid Label System

### On REST API

To set Labels you have to use the `labels` directive in the body of any IndexCard request:
 ```json
{
    "labels": [
        "#tag",     // tag
        "###tag2",  // tag2
        "tag3",      // tag3
        "#person=name=John Doe",
        "#person=birthday=2020=April=25",
        { // ..or like this equivalent..
            "person": {
                "name": "John Doe",
                "birthday": "2020=April=25"
            }
        }
    ]
}
```

Felix, einfach .split über #,
leere überlesen, dann split über =,  // das kannst vielleicht sogar rausnehmen und nur = verwenden, letztes als Value.
leere überlesen, dann split über =,
wenn größer 1, dann key-value (Label), sonst nur key (Tag).



### On File System

- `#tag` - Tags in file-names to be used as attributes
- `#can also be more `
- `#person=birthday=2020=April=25`
    - associates this years #myBirthday Tag with The file.
    - allows you to find the file below #birthday, as it is just a label itself.
- _`#1234` - Numbers are also allowed, but generally not advisable as primary Tags._
    - Rather use them as secondary Tags: `#day#23#`

- Not allowed..
    - `#\n\t` - escape sequences of some sort.
    - `#`, `=`,`=` - reserved symbols (for now).
    - All the following symbols: `:`, `;`, `?`, `&`, `*`, `+`, `^`, `|`, `\`, `/`, `<`,`>`, `"`.

- Please Note:
    - You should not use this feature with legacy filesystems, as it might break your tagging.
    - *Although in most cases it should be fine due to Unicode conversion, as long as the length is not cut off.*

The `.va` file extension has to be present in order to let VA organize your file,
although it just has to be **present in path**, not filename to be recognized.

## Contribution

