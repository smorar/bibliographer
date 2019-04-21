#MCS_FLAGS += -nowarn:0169
#MCS_FLAGS += -unsafe+

CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS) $(GTKSHARP_LIBS) $(NEWTONSOFTJSON_LIBS)

ASSEMBLY_NAME = bibliographer
ASSEMBLY = $(ASSEMBLY_NAME).exe
WRAPPER = $(ASSEMBLY_NAME)

#if ENABLE_TESTS
#ASSEMBLY_TEST = $(ASSEMBLY_NAME).test.exe

#TESTSOURCES = 
#endif

SOURCES = 								\
	$(srcdir)/widgets/SearchEntry.cs		\
	$(srcdir)/AboutBox.cs				\
	$(srcdir)/Alteration.cs				\
	$(srcdir)/AssemblyInfo.cs			\
	$(srcdir)/BibliographerChooseColumns.cs		\
	$(srcdir)/BibliographerLitTreeView.cs		\
	$(srcdir)/BibliographerMainWindow.cs		\
	$(srcdir)/BibliographerSearchEntry.cs		\
	$(srcdir)/BibliographerSettings.cs		\
	$(srcdir)/BibliographerSidePaneTreeView.cs	\
    $(srcdir)/bibliographerSplashScreen.cs  \
	$(srcdir)/BibliographerStartup.cs		\
	$(srcdir)/Cache.cs				\
	$(srcdir)/Debug.cs				\
	$(srcdir)/FileIndexer.cs			\
	$(srcdir)/LitListStore.cs			\
	$(srcdir)/LookupRecordData.cs			\
	$(srcdir)/SidePaneTreeStore.cs			\
	$(srcdir)/StringOps.cs				\
    $(srcdir)/SuperTri.cs               \
	$(srcdir)/ThumbGen.cs				\
	$(srcdir)/Tri.cs

RESOURCES =							\
	-resource:$(top_srcdir)/resources/bibliographer.glade	\
	-resource:$(top_srcdir)/resources/bibliographer.png	\
	-resource:$(top_srcdir)/resources/error.png		\
	-r:/usr/lib/mono/4.0/System.Runtime.Serialization.dll	\
	-r:/usr/lib/mono/4.0/Mono.Posix.dll			\
	-r:$(top_srcdir)/libbibby/libbibby.dll

$(WRAPPER): $(srcdir)/$(WRAPPER).in
	sed -e "s|\@prefix\@|$(prefix)|g"               \
	    -e "s|\@pkglibdir\@|$(pkglibdir)|g"         \
	    -e "s|\@target\@|$(ASSEMBLY)|g"               \
	    -e "s|\@mono\@|$(MONO)|g"                   \
	    -e "s|\@mono_flags\@|$(MONO_FLAGS)|g"       \
	    < $^ > $@

$(ASSEMBLY).config: $(srcdir)/$(ASSEMBLY).config.in
	sed -e "s|\@pkglibdir\@|$(pkglibdir)|" \
	< $^ > $@

$(ASSEMBLY): $(SOURCES)
	$(CSC) -out:$@ $(SOURCES) $(RESOURCES)

#if ENABLE_TESTS

#$(ASSEMBLY_TEST): $(SOURCES_TEST)
#        $(CSC) -out:$@ $(SOURCES_TEST) $(RESOURCES) $(NUNIT_LIBS)

#all: $(ASSEMBLY) $(ASSEMBLY_TEST)

#test: all
#        $(NUNIT) $(ASSEMBLY_TEST)

#EXTRA_DIST = $(SOURCES) 			\
#	$(SOURCES_TEST)				\
#	$(top_srcdir)/gtk-gui/gui.stetic 	\
#	bibliographer.in bibliographer.exe.config.in

#CLEANFILES = 					\
#	$(ASSEMBLY)				\
#	$(ASSEMBLY).config			\
#	$(ASSEMBLY_TEST)			\
#	$(WRAPPER)

#else

all: $(ASSEMBLY)

EXTRA_DIST = $(SOURCES) \
	bibliographer.in bibliographer.exe.config.in

CLEANFILES = 					\
	$(ASSEMBLY)				\
	$(ASSEMBLY).config			\
	$(WRAPPER)

#endif

check-syntax:
	$(MAKE) syntax-target SYNTAX="-fsyntax-only"

syntax-target: $(ASSEMBLY)

bibliographerdir = $(pkglibdir)
bibliographer_DATA = $(ASSEMBLY) $(ASSEMBLY).config

bin_SCRIPTS = $(WRAPPER)

MAINTAINERCLEANFILES = \
	Makefile.in
