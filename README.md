# Splitloader

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
It will be used to find a suitable FFmpeg installation on the user's system, otherwise it can download one that it can use.
Using the list of video files provided by the UI and FFmpeg, it will produce a concatenated video file without re-encoding, which will then be handed to the uploader.

#### Splitloader.Uploader (not yet implemented)

This will handle the actual interfacing with the Youtube API to upload the video.
I may implement this is an abstraction layer that defines an interface `IUploadService`, which could be used with any number of video services besides just Youtube.
This would mean that there would be Splitloader.UploadService.* assemblies for each potential upload service, each of which would implement IUploadService.
Youtube would still be the first to be implemented (selfish dev is thinking about his own needs first!), but ideally this could be built as a plugin system where each Splitloader.UploadService dll would be loaded dynamically at runtime, allowing the user to freely switch between any installed backend, and developers to implement whichever services they wish.

## Progress

At the moment, this is just a functional UI.
The file picker will populate the list, but none of the backend logic is implemented at this point.

## What's next?

Backend logic, obviously!
Getting the FFmpeg interop happening is the next priority, then I'll turn to the uploader last.
