SUBDIRS = 		\
	libbibby	\
	src

desktopdir = $(datadir)/applications
desktop_DATA = resources/bibliographer.desktop

iconsdir = $(datadir)/pixmaps
icons_DATA = resources/bibliographer.png

schemadir = $(datadir)/glib-2.0/schemas
schema_DATA = resources/apps.bibliographer.gschema.xml

man1_MANS = resources/bibliographer.man

EXTRA_DIST = resources/bibliographer.png 	\
	resources/bibliographer.desktop 		\
	resources/bibliographer.man 			\
	resources/error.png						\
	resources/apps.bibliographer.gschema.xml

DISTCLEANFILES = 	\
	*.bak		\
	*~		\
	obj		\
	bin		\
	*.pidb

MAINTAINERCLEANFILES = 	\
	aclocal.m4 	\
	ltmain.sh 	\
	Makefile.in 	\
	configure 	\
	config.sub 	\
	config.guess
	
