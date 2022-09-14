# Clear Water

A simple water shader for Unity/VRchat that (ab)uses some Unity features to get nice, clean looking water. 

![Preview](https://files.catbox.moe/ida0lh.jpg)

## Installation

Download the repository. Then place the Shader/ folder with the shader into your Assets/ directory.

## Usage

There are two variants of the shader included - a normal and Cheap variant. The Cheap variant doesn't use a grab pass, so it's great for avatars and expansive worlds!

Place a [water normal map](https://www.google.com/search?q=water+normal+maps&tbm=isch) into the Wave texture slots and a foam texture into the foam slot. Then play around with the options until it looks nice. 

Note that, like other water shaders with a depth fade effect, you'll need a directional light or other depth buffer activator to see the depth fade effect

## UI is weird!

Probably will be fixed later.

## The default settings are broken!

Play around and see what works!

I recommend lowering the Wave Strength values to be low and inverse of each other. 

## License?

This work is licensed under a [Creative Commons Attribution-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-sa/4.0/)