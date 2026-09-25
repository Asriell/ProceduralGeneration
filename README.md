# Génération Procédurale de Terrain

Projet Unity d'apprentissage pour générer des terrains procéduraux infinis à l'aide du bruit de Perlin fractal (FBM).

---

## Fonctionnalités

- **Heightmap Perlin** — génération d'une carte de hauteurs en empilant plusieurs octaves de bruit de Perlin (Fractional Brownian Motion).
- **Biomes colorés** — attribution d'une couleur par couche de hauteur (eau, sable, herbe, roche, neige…) via des `LandscapeType` sérialisables.
- **Génération de mesh** — construction d'un mesh quad-grid à partir de la heightmap, avec courbe d'animation pour le profil d'élévation et niveaux de détail (LOD 0–6).
- **Terrain infini** — streaming de chunks à la volée autour du joueur ; les chunks hors de la distance de vue sont désactivés pour économiser les draw calls.
- **Caméra vol libre** — navigation WASD/QE + souris pour inspecter le terrain en mode Play.
- **Prévisualisation éditeur** — bouton *Generate* dans l'Inspector avec `autoUpdate` optionnel.

---

## Architecture

```
Assets/Scripts/
├── Util.cs          # Bibliothèque statique : bruit, textures, meshes
├── Landscape.cs     # Générateur mono-chunk (outil éditeur / preview)
├── EndlessMap.cs    # Terrain infini par streaming de chunks
├── MapDisplay.cs    # Adaptateur entre le pipeline et les renderers Unity
└── FlyCamera.cs     # Caméra de vol libre pour exploration en Play mode

Assets/Editor/
└── SceneSetup.cs    # Script d'initialisation de scène
```

### Pipeline de génération

```
seed + paramètres
       │
       ▼
 Util.CreatePerlinNoiseMap()   →  float[,]  (heightmap [0,1])
       │
       ├──▶  Util.textureGenerator()        →  Texture2D (couleur ou niveaux de gris)
       │
       └──▶  Util.GenerateMesh()            →  MeshDatas
                    │
                    └──▶  MeshDatas.CreateMesh()  →  Mesh Unity
```

---

## Paramètres clés (`Landscape` / `EndlessMap`)

| Paramètre | Rôle |
|---|---|
| `scale` | Zoom du champ de bruit — plus grand = features plus larges |
| `octaves` | Nombre de couches de bruit empilées — plus = détail fin |
| `persistence` | Facteur d'amplitude par octave (0–1) — plus bas = terrain plus doux |
| `lacunarity` | Facteur de fréquence par octave (≥1) — plus haut = plus de haute fréquence |
| `seed` | Graine aléatoire — même seed → même carte |
| `heightRateMesh` | Multiplicateur vertical du mesh |
| `heightCurve` | Courbe d'animation pour un profil d'élévation non-linéaire |
| `levelOfDetail` | 0 = résolution max ; 1-6 = maillage progressivement simplifié |

---

## Lancer le projet

1. Ouvrir le dossier `ProceduralLandscape/` dans **Unity 6** (ou la version indiquée dans `ProjectSettings/ProjectVersion.txt`).
2. Ouvrir la scène principale dans `Assets/Scenes/`.
3. Sélectionner le GameObject **Landscape** et cliquer sur **Generate** dans l'Inspector pour une prévisualisation statique, ou lancer le Play mode pour le terrain infini (`EndlessMap`).
4. En Play mode, utiliser **WASD / QE** pour se déplacer et la **souris** pour regarder. **Échap** déverrouille le curseur.

---

## Technologies

- Unity 6 (C#)
- `Mathf.PerlinNoise` pour le bruit de base
- `AnimationCurve` pour le remapping d'élévation
- AI Navigation package (présent dans le projet, non utilisé activement)

