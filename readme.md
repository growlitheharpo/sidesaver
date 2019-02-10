# Sidesaver - A Simple Backup Engine

*Sidesaver* is a simple Windows application for creating iterative backup saves in programs that don't natively support that (ex. Adobe Photoshop).

*MVP - Minimum Viable Product.* 
> "Just enough features to satisfy early customers, and to provide feedback for future product development." - Wikipedia

This is a very early version of Sidesaver with just enough functionality to support a simple workflow. It will run in the background while you work. Everytime you save, it will copy the file, and save the new copy as a backup.

* The user can add files to the Sidesaver watch list to specify which files should be backed up
* The user can specify whether Sidesaver should run in the background from the tray when closed
* The user can specify a particular "backup location" where all backups will be saved (if unspecified, they will be saved side-by-side with the original)
* The user can choose how many backups to save (specifying 1 to 25, or an "infinite" number)
* Sidesaver will reconcile backup states between program instances, using the ".backup" extension as a way to track what it previously did
* User settings are persisted across sessions

Sidesaver is still pretty simple and does [exactly what it says on the tin](https://tvtropes.org/pmwiki/pmwiki.php/Main/ExactlyWhatItSaysOnTheTin). 

Some big limitations:
* It will not remember which files it was watching between program sessions.
* You cannot specify the naming convention of the backup files (yet!)
