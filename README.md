# Overview

Sonos Core (SC) is a ASP .Net 6 Web Application to Manage your Sonos Devices for your Local Library or played Streams

I have wrote it for me because I missed for a long Time the ID3 Tag Support from Sonos. 
So now I have my own little Monster of Sonos App. 
My Primary Language is German so the Ui is with german terms but I think if you need it we can speak together.
Pictures can be found here: https://github.com/talun2075/SonosCore/tree/master/ExcamplePictures

## What can it not
It is not there to replace the Sonos own App. 
- You can not Add new Devices over SC.
- You can not Browse Streams
- You can not change Settings where Sonos is used the new Cloud APi for. Example change the maximum Volume for one Player.
- No multilanguage Support (jet)


## What can it
- You can Group you Player
- You can use old Software Version and new Software Version in one Interface.
--  With the limit, that you only group Player with same Softwareversion
- If your local music used ID3 Tag you can get all Tags you wish
-- Lyrics
-- Rating (PopM)
-- Mood
-- Speed
-- Composer
- You can rate your own files with PopM, Mood, Speed etc.
- You can Browse your own Library for Playlists, Artist, Genres, Favorites. On Browsing you see Albumart and ID3 Tags
- You can add and edit Playlist with an export to M3U
- You can manage your Alarms
- You can manage a sleep Timer
 
# How it works
The Code based on 

- https://github.com/jishi/Jishi.Intel.SonosUPnP that is based on the Intel UPNP Library which is not longer supported.
- The managing of the ID3 Tag is used over a TagLib port to .NET. 
- NLog is used for Logging

I have the Intel and TagLib in my Source code and update them if I found Errors. All are .NET 5 or higher

The Taglib Project and my used wrapper is stored in the 
https://github.com/talun2075/HomeCommon
Solution. You will need this Solution if you whant to use SC because they have the needed refferences.

## How tho Start
You need Visual Studio 2022.
Get latest of https://github.com/talun2075/HomeCommon and SC.
After Build I hope you have no Errors and can Start.
On Windows your Firewall Manager ask for permissions you have to grant. 
If you whant to modify your local music the running user need permissions to the files of your music. 

After this call the url and enjoy ;-)

On Linux I have no Idea ;-)

## And then? 
I have add many different features for me and my Family. 
So the Most Controllers you will never need but if you want to see some features give me a ping.