# Splitloader

![Screenshot](screenshot.png)

Splitloader (Split-Video Uploader... get it? Pretty creative if I do say so myself :laughing:) is a simple app that automates the processes of concatenating split video files and uploading them to Youtube (or other sources in the future).

## Purpose

Splitloader is meant to make my life easier.
I manage the Youtube page for my church (check it out [here](https://www.youtube.com/@stelizabethorthodoxpoulsbo)!), and so this is simply meant to eliminate the manual steps of concatenating the video files our GoPro produces and doing the upload by using Google's Youtube API.

## Architecture

#### Splitloader.UI

This is the main UI of the app.
It gets the video name, description, and list of video files from the user, then hands that information off to the backend for processing and uploading.
Progress updates are reported back to the interface.

#### Splitloader.VideoTools

This is the interop layer between Splitloader and FFmpeg.
It is used to find an FFmpeg installation on the user's system, or download one that it can use.
Using the list of video files provided by the UI and FFmpeg, it will produce a concatenated video file without re-encoding, which will then be handed to the uploader.

#### Splitloader.Uploader (not yet implemented)

This will handle the actual interfacing with the Youtube API to upload the video.
I may implement this is an abstraction layer that defines an interface `IUploadService`, which could be used with any number of video services besides just Youtube.
This would mean that there would be Splitloader.UploadService.* assemblies for each potential upload service, each of which would implement IUploadService.
Youtube would still be the first to be implemented (selfish dev is thinking about his own needs first!), but ideally this could be built as a plugin system where each Splitloader.UploadService dll would be loaded dynamically at runtime, allowing the user to freely switch between any installed backend, and developers to implement whichever services they wish.

## Progress

The app correctly finds FFmpeg installed on the system, or downloads it, and then uses it to create a concatenated video file.
**All of this is only tested on Linux.
It's meant to run on Windows as well, but that hasn't been tested and is probably broken.
There is no Mac compatability at the moment.**

## What's next?

The Youtube uploader needs to be written next.
I'd also like to add in to QoL features, like verifying that the selected video files _can_ actually be concatenated, and other such things like that.
