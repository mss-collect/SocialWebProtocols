# SocialWebProtocols

Prototypes and playground for ActivityStreams, ActivityPub, WebMentions etc.

## Sources "stolen" (MIT License) from

- https://github.com/KristofferStrube
  - ActivityStream
  - ActivityPub


## TODO

"Object" und "ObjectOrLink" oder auch "class" sind im Umfeld von .Net und C# keine guten Begriffe, da bereits stark verwendet.

Wie können diese Ersetzt werden ohne komplett gegen die Begrifflichkeiten aus [Activity Streams 2.0](https://www.w3.org/TR/activitystreams-core/)

"Object" und "Link" sind dort zentrale Begriffe, aber auch noch einige andere.

- Object
- Link
- Activity
- IntransitiveActivity
- Collection
- OrderedCollection
- CollectionPage

Siehe [Core Types](https://www.w3.org/TR/activitystreams-vocabulary/) 

Herausforderung: Es gibt noch ca. 50 "Extended types", das meiste sind Activity Subtypes.
Diese sind im Repo von K.Strubbe bereits ausimplementiert, das neu zu schreiben will ich mir ersparen.
Idee: die Extended gar nicht umbenennen, da die Namen nicht so sehr mit den C# Keywords kollidieren

### Idee

Die Anzahl der Begriffe für die Core Typesist mit sieben sehr überschaubar und seit über 5 Jahren konstant.

Komplettes umbenennen von Object und Link erzeugt viel Verwirrung.

Ein Prefix (oder Suffix?) wäre ein gangbarere Kompromiss.

Die gemeinsame Basisklasse für Object und Link könnte sogar "nur" der Prefix sein.

#### Kandidaten

- ActivityStream
- AS
- ACST
- Core Types (vs. Extended Types)
- Core Class
- Core



- CoreObject
- CoreLink
- CoreActivity
- CoreIntransitiveActivity
- CoreCollection
- CoreOrderedCollection
- CoreCollectionPage

Die gemeinsame Basisklasse (LinkOrObject) könnte "Core" heissen

Namespace: ActivityStreams (Plural, sonst nicht vorkommend)

Verfeinerung: das Prefix kommt nur für Link und Object (das HierarchieLevel und vorher, nicht mehr danach!)
Hierarchie:

- Core
  - CoreLink
  - CoreObject
    - CoreActivity
      - CoreIntransitiveActivity
    - CoreCollection
      - CoreOrderedCollection

- Core
  - CoreLink
  - CoreObject
    - Activity
      - IntransitiveActivity
    - Collection
      - OrderedCollection
      - CollectionPage

## Wie kann ich ActivityPub in Gang setzen ?

- Es scheit einen öffentlichen Server zum Testen von Implementierungen zu geben
- Bei öffentlichen Instanzen zumindest mal lauschen (Mastodon)

[https://socialhub.activitypub.rocks/t/status-of-a-robust-activitypub-test-suite/3310](https://socialhub.activitypub.rocks/t/status-of-a-robust-activitypub-test-suite/3310)

[https://feditest.org/](https://feditest.org/)
