NbuExplorer
===================

Nokia NBU, NFB and NFC backup file (produced by Nokia Content Copier) and ARC
backup file (created by symbian phones as backup to memory card) parser,
extractor and viewer. It can help you check the contents of a backup or extract
files from it. It is non-destructive as it accesses backup files in a read-only
mode. Nevertheless, it is provided with NO WARRANTY (according to GNU GPL
license).
The main goal of this application is to enable Nokia phone and Nokia software
(PC Suite and Ovi Suite) users to access their data stored in backup files
in as much comfortable way as possible. This application IS NOT an official
Nokia product and can be used only at your own risk.

Main features
===================
- View content of nbu, nfb, nfc and arc backup files
- Extract individual files from backup files
- Brute force scan for vcard phone data in any file

System requirements
===================
To run this application you need to have installed MS .Net Framework 2.0
(or newer) runtime. It can be downloaded from official MS download pages:
http://www.microsoft.com/downloads/details.aspx?FamilyID=0856eacb-4362-4b0d-8edd-aab15c5e04f5

The application should also run under the Mono runtime. It was successfully
tested on Ubuntu 9.10 after installing the winforms2.0 extension (can be done
with the following command: "sudo apt-get install libmono-winforms2.0-cil").

Usage
===================
Open
-------------------
Simply open a backup file either by menu item File/Open, or drag&drop backup
file onto the running application main form or by launching application with
a path to a backup file as command line argument ("open with" association to
*.nbu or any other file extension is possible too).

Browse
-------------------
Once in the main program window, you should see recognized contents in the form
of a directory tree on the left part of application main form. You can browse
that folder structure as you are probably used to from standard applications.
The right top part displays a list of files in a selected folder and optional
(hid-able) right bottom part works as a prieview of the selected file. It can
display pictures (common formats like jpg, png, gif) and text (recognized only
by file extensions - txt, htm, jad,...).

Export
-------------------
All recognized files can be exported either by a double-click on the selected
file in the file list, via context menu in file list or directory tree. When you
select only one file to export, you are prompted to select a target directory
and are allowed to change the file name. When you select more files or a whole
directory, you are prompted to select a target directory. An overwrite dialog
may appear when some "existing file collision" occurs.

File parsing log
-------------------
The secondary tab of the application main form called "File parsing log" can be
used to check details about the backup file parsing process. Hexadecimal numbers
appearing in the log are usually start addresses (from beginning of file)
of blocks that were detected in the backup file.
File coverage parameter which should appear at the end of each successful
parsing log means how large part of the whole backup file (in percents) has been
understood and can be viewed or extracted as files.

Bruteforce scanning
-------------------
Important data like contacts, messages and calendar items are stored in nbu 
backups in the format of vcards, which can be typically identified by starting
and ending text sequence (BEGIN:VCARD...END:VCARD). Therefore, it  is possible
to search for such data in any file without understanding its internal
structure. This method can be used for corrupted backups or backups on which
the standard method fails. It is also suitable for different (than nbu) file
formats which preserve data in vcard form (for example communication center
cache files such as PCCSContact.db, PCCSSMS.db).
Bruteforce mode is automatically used when opening a file with other extensions
than nbu, nfb or nfc. For nbu files, it will be used when option "All files
(bruteforce scan)" in the file type filter of open file dialog is  selected.

Contact
===================
Project homepage: http://sourceforge.net/projects/nbuexplorer
SVN repository: https://nbuexplorer.svn.sourceforge.net/svnroot/nbuexplorer
Donation page: http://sourceforge.net/donate/index.php?group_id=281139
Author: Petr Vilem, petrusek@seznam.cz
