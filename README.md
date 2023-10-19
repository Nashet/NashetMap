# Nashet map generator

Can generate random maps or load map from png file. Includes camera control, patch finder, province name generation, flag generation, map clicks handler and example with rivers and country borders

Originally used for https://github.com/Nashet/Prosperity-Wars.

Usage example is in in \Samples\\. It includes river generation, country borders, mountain borders(impassable), camera control, map clicks handler, units movement.

Another example is https://github.com/Nashet/MapNECS.git.

Meshes are not optimized. Thought, you can use vertex shader for really voluminous mountains.

![Example Image](https://i.imgur.com/EhMcrCP.png)

# Installation
## As unity module
This repository can be installed as unity module directly from git url. In this way new line should be added to Packages/manifest.json:

"com.nashet.map": "https://github.com/Nashet/NashetMap.git",

By default last released version will be used. If you need trunk / developing version then develop name of branch should be added after hash:

"com.nashet.map": "https://github.com/Nashet/NashetMap.git#develop",

