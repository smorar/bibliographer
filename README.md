<img src="https://github.com/smorar/bibliographer/raw/master/resources/bibliographer.png" align="right" />

# Bibliographer

Bibliographer is a reference and literature management application. It catalogues literature and supports the following features:
 * Fast full text search of entire literature database
 * Automatic look-up of metadata for documents that contain a valid DOI link
 * Meta-data stored in a sqlite database, and changes occur seamlessly

## Installation:

Requirements:
 * Mono => 4.0
 * gtk-sharp => 2.99
 * sqlite
 * automake1.9
 * autoconf

Installation instructions:

From a GIT checkout:
Requirements:

```        
$ ./bootstrap
$ ./configure
$ make
```
and, as root:
```
# make install
```
        
From a tar.gz release:
```
$ ./configure
$ make
```
and, as root:
```
# make install
```

## Authors

 * Sameer Morar <smorar@gmail.com> (present maintaner)
 * Carl Hultquist <chultquist@gmail.com>

## Future work

 * Automatic file management:
 ** Store all literature files in a directory tree. User customisable prefix, then in the following folders, based on type:
    $PREFIX$/Papers, $PREFIX$/Books, $PREFIX$/Thesis, $PREFIX$/Article etc...
 ** Files to be renamed in the following format, based upon database entry:
    YEAR - FIRST AUTHOR - TITLE.ext
 * Bibtex + other types of bibliographic database files to be exported by the application.
 * Provide a tagging mechanism to group / classify certain topics together.
 * Literature indexing:
 ** Support for Abiword and OpenOffice documents
 * Addition of user meta-data eg. notes, abstracts, reviews
 * Drag and drop support for references:
 ** Be able to drag from list view and drop a reference into a document being edited
 * Word processor integration with Lyx, Abiword, OpenOffice, MS-Word (Win port)
 * Online literature database searching eg. Medline, ScienceDirect etc...
 * Win32 port
 
 ## License
 
 This project is licensed under the Gnu GPL version 2 license.