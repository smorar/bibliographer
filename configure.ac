
AM_CONFIG_HEADER(config.h)

AC_INIT(bibliographer, 0.1.3)
AC_CONFIG_SRCDIR(src/BibliographerStartup.cs)
AM_INIT_AUTOMAKE

AM_MAINTAINER_MODE

MONO_REQUIRED=2.0
GTKSHARP_REQUIRED=2.12

PKG_CHECK_MODULES(MONO,
	mono >= $MONO_REQUIRED)

PKG_CHECK_MODULES(GTKSHARP,
	gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)

NUNIT_REQUIRED=2.4.7

AC_ARG_ENABLE(tests, AC_HELP_STRING([--enable-tests], [Enable NUnit tests]), enable_tests=$enableval, enable_tests="no")

if test "x$enable_tests" = "xno"; then
	do_tests=no
	AM_CONDITIONAL(ENABLE_TESTS, false)
else
	PKG_CHECK_MODULES(NUNIT, nunit >= $NUNIT_REQUIRED,
	do_tests="yes", do_tests="no")

	AC_SUBST(NUNIT_LIBS)
	AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")
	AC_PATH_PROG(NUNIT, nunit-console)

	if test "x$do_tests" = "xno"; then
		PKG_CHECK_MODULES(NUNIT, mono-nunit >= 2.4,
		do_tests="yes", do_tests="no")

		AC_SUBST(NUNIT_LIBS)
		AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")
		AC_PATH_PROG(NUNIT, nunit-console)

		if test "x$do_tests" = "xno"; then
			AC_MSG_WARN([Could not find nunit: tests will not be available]) 
		fi
	fi
fi

AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AC_PATH_PROG(MONO, mono)
AC_PATH_PROGS(MCS, [gmcs gmcs2])

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

AC_OUTPUT([
Makefile
libbibby/Makefile
src/Makefile
])
