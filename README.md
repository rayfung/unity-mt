# Unity3D multi-touch for Linux X11
## Introduction
This is a native plugin (libmt) which adds multi-touch support to Unity3D Linux player under X11.  
This repository includes libmt plugin and unity sample code.
## Building
In order to build libmt plugin, you need to install development package of X11 and Xi.  
You also need to change the fullscreen mode of your unity project to "Windowed" in "Project Settings > Player > Resolution and Presentation"  
## Running
This plugin does not support regular desktop environment (currently). The unity player must be run in a custom xsession (/usr/share/xsessions/foobar.desktop):

```
[Desktop Entry]
Name=foobar
Exec=/path/to/unity_player.run
Type=Application
```
## Links
* [XI2 Recipes, Part 1](https://who-t.blogspot.com/2009/05/xi2-recipes-part-1.html)
* [Multitouch in X - Getting events](https://who-t.blogspot.com/2011/12/multitouch-in-x-getting-events.html)
* [XI2 Protocol](https://www.x.org/releases/current/doc/inputproto/XI2proto.txt)
* [test_xi2.c](https://gitlab.freedesktop.org/xorg/app/xinput/-/blob/master/src/test_xi2.c)
