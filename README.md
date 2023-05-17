

![header](./.github/images/header.png)
---
| Build                                                                                                                                                   |
|---------------------------------------------------------------------------------------------------------------------------------------------------------|
| [![ci](https://github.com/jdaar/FsBuddy/actions/workflows/ci.yaml/badge.svg?branch=master)](https://github.com/jdaar/FsBuddy/actions/workflows/ci.yaml) |

An open-source platform for filesystem automation provides a user-friendly graphical user interface (GUI) that is designed to simplify the automation of various filesystem tasks. **Windows only app, Linux on roadmap**.

## Demo

[Go to demo (Youtube)](https://youtu.be/hYKSo_WblS4)

## Screenshots

![main_screenshot](./.github/images/main_screenshot.png)

## Features
- Subscribe to a directory file changes.
- Set filter by filename.
- Move file on creation and rename.

## Installation

Go to the [Releases](https://github.com/jdaar/FsBuddy/releases) section and download the file **Setup-x64.exe**, then open the file as an Administrator and follow the setup wizard.

## Usage

The following pictures will show you the basic structure of the UI:

### Header

![header_explanation](./.github/images/header_explanation.png)

### Watcher list

![watcherlist_explanation](./.github/images/watcherlist_explanation.png)

### Create/Edit

![create_edit_explanation](./.github/images/create_edit_explanation.png)

## Roadmap

### Milestones

- [ ] Javascript VM implementation, managed threads and anonymous pipe implementation
- [ ] User-defined WatcherActions
    - [ ] Allow the watcher to perform a custom action when activated (after Javascript VM implementation)
- [ ] Filters
    - [ ] User-defined WatcherAction (after Javascript VM implementation)
- [ ] File data extraction
    - [ ] Export to csv
    - [ ] CRUD table with LiteDB (after migration)
    - [ ] User-defined tables with LiteDB
    - [ ] Data insertion to user-defined tables
- [ ] Add support for Linux
- [ ] Migrate from SQLite to LiteDB
    - [ ] Reimplement ThreadManager with LiteDB as datasource (this is because of LiteDB's thread safety 

### Features

- [ ] Log visualization from within the app


 
