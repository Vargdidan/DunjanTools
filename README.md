### Verified working with Godot C# v3.2.2

# DunjanTools
A minimalistic Virtual Tabletop.

# Goal
Have a embedded, easy to use VTT, that is not bogged down with overwhelmingy many features.

DunjanTools were ment as a supplement to dice-rollers, discord/chat programs and different ruleset encylopedias. However, if we find a good way to add these features without making a massive and complicated GUI, go for it! It may be feature rich in the future, but new features should not come in the way for new users to simply join a game, drop tokens and navigate the battlemap.

# Code-talk
I am a "old-fashioned" programmer, I have been used to and molded by C/C++. GDScript is a awesome prototype language, but to be frank, I need the structure and capabilities of a widespread programming language. This does not mean that you cannot contribute with GDScripts! Even though we are building with mono/C#, GDScripts can cooexist.
If you are only used to GDScript projects, building this project might throw you off for a couple of minutes. But the tradeoff is in my eyes worth it.

BeIndie explains and goes through the process in a good way I think, so watch this and read the Godot docs for C#.
https://www.youtube.com/watch?v=ra-BJ-fJ6Qo&t=241s 

## Problems running?
Sometimes I have had an issue with opening the project when I swapped computers. Might be that I have dragged with me some local-settings into the repository. It worked once I went into the project.godot file, and removed "run/main_scene="res://GUI/MainMenu.tscn"" under [Application]. Then it usually opens up and you can re-set the main_scene.

### Trello:
https://trello.com/b/HMsWSU4w/tabledungeon

### Color palette (I'm not a great designer, but I love consistency):
https://coolors.co/264653-2a9d8f-e9c46a-f4a261-e76f51
