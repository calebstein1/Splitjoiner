# Splitloader

![Screenshot](screenshot.png)

Splitloader (Split-Video Uploader... get it? Pretty creative if I do say so myself :laughing:) is a simple app that automates the processes of concatenating split video files and uploading them to Youtube (or other sources in the future).

## Purpose

Splitloader is meant to make my life easier.
I manage the YouTube page for my church (check it out [here](https://www.youtube.com/@stelizabethorthodoxpoulsbo)!), and so this is simply meant to eliminate the manual steps of concatenating the video files our GoPro produces and doing the upload by using Google's YouTube API.

## Architecture

#### Splitloader.UI

This is the main UI of the app.
It gets the video name, description, and list of video files from the user, then hands that information off to the backend for processing and uploading.
Progress updates are reported back to the interface.

#### Splitloader.VideoTools

This is the interop layer between Splitloader and FFmpeg.
It is used to find an FFmpeg installation on the user's system, or download one that it can use.
Using the list of video files provided by the UI and FFmpeg, it will produce a concatenated video file without re-encoding, which will then be handed to the uploader.

#### Splitloader.UploadServices.Common

This contains definitions that are common across any video platform and can be used by other Splitloader.UploadServices assemblies in the future.
The reason for keeping this as a separate assembly is exactly to allow for other services to be added more easily.

#### Splitloader.UploadServices.YouTube

This handles the interactions with Google's YouTube API.
It, when provided with the JSON file Google provides for OAuth2, uploads the concatenated video file created by VideoTools along with the title and description set in the UI.
Upload progress is reported back to the status bar in the UI.

## Progress

All the basic functionality is there (on Linux).
Splitloader correctly finds or downloads FFmpeg, concatenates video files, and uploads the concatenated file to YouTube.
**All of this is only tested on Linux.
It's meant to run on Windows as well, but that hasn't been tested and is probably broken.
There is no Mac compatability at the moment.**

## What's next?

The next steps, now that the core functionality is done, include decoupling the YouTube uploader from the UI so that more services can be added; and adding proper cross-platform support, rather than just being Linux-only as it is now.