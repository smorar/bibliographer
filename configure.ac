AC_INIT(src/BibliographerStartup.cs)

AM_CONFIG_HEADER(config.h)

AM_INIT_AUTOMAKE(bibliographer, 0.1)

AM_MAINTAINER_MODE

MONO_REQUIRED=1.0
GTKSHARP_REQUIRED=2.0

PKG_CHECK_MODULES(MONO,
	mono >= $MONO_REQUIRED)

PKG_CHECK_MODULES(GTKSHARP,
	gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
	glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)

AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
#AM_GCONF_SOURCE_2

AC_PATH_PROG(MONO, mono)
AC_PATH_PROG(MCS, gmcs)

CS="C#"
if test "$MCS" = "no" ; then
	AC_MSG_ERROR([No $CS compiler found])
fi

MCS_FLAGS="-codepage:utf8"

MONO_FLAGS=
if test $USE_MAINTAINER_MODE = yes; then
	MCS_FLAGS="$MCS_FLAGS -debug"
	MONO_FLAGS="$MONO_FLAGS --debug"
fi

AC_SUBST(MCS_FLAGS)
AC_SUBST(MONO_FLAGS)

dnl GAC
AC_PATH_PROG(GACUTIL, gacutil)
if test x$GACUTIL = x; then
        AC_MSG_ERROR(You need gacutil)
fi

#GACUTIL_FLAGS='/package muine /gacdir $(libdir) /root $(DESTDIR)$(libdir)'
#AC_SUBST(GACUTIL_FLAGS)

AC_OUTPUT([
Makefile
src/Makefile
])
