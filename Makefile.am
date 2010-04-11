SUBDIRS = src \
	libbibby

desktopdir = $(datadir)/applications
desktop_DATA = resources/bibliographer.desktop

iconsdir = $(datadir)/pixmaps
icons_DATA = resources/bibliographer.png

man1_MANS = resources/bibliographer.man

EXTRA_DIST = resources/bibliographer.png \
	resources/bibliographer.desktop \
	resources/bibliographer.man \
	resources/error.png

DISTCLEANFILES = \
	*.bak
	*~
	*.pidb

MAINTAINERCLEANFILES = \
	aclocal.m4 \
	ltmain.sh \
	Makefile.in \
	configure \
	config.sub \
	config.guess
	
