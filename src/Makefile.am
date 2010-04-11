MCS_FLAGS += -nowarn:0169
MCS_FLAGS += -unsafe+
CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS)

TARGET = bibliographer.exe
WRAPPER = bibliographer

BIBLIOGRAPHER_CSFILES = 				\
	$(top_srcdir)/gtk-gui/bibliographer.BibliographerMainWindow.cs 	\
	$(top_srcdir)/gtk-gui/bibliographer.BibliographerChooseColumns.cs \
	$(top_srcdir)/gtk-gui/bibliographer.BibtexEntryDialog.cs 	\
	$(top_srcdir)/gtk-gui/generated.cs		\
	$(srcdir)/widgets/HoverImageButton.cs		\
	$(srcdir)/widgets/SearchEntry.cs		\
	$(srcdir)/AboutBox.cs				\
	$(srcdir)/Alteration.cs				\
	$(srcdir)/AssemblyInfo.cs			\
	$(srcdir)/BibliographerChooseColumns.cs		\
	$(srcdir)/BibliographerLitTreeView.cs		\
	$(srcdir)/BibliographerMainWindow.cs		\
	$(srcdir)/BibliographerSearchEntry.cs		\
	$(srcdir)/BibliographerSidePaneTreeView.cs	\
	$(srcdir)/BibliographerStartup.cs		\
	$(srcdir)/BibtexEntryDialog.cs			\
	$(srcdir)/Cache.cs				\
	$(srcdir)/Config.cs				\
  	$(srcdir)/Debug.cs				\
	$(srcdir)/FileIndexer.cs			\
	$(srcdir)/LitListStore.cs			\
	$(srcdir)/LookupRecordData.cs			\
	$(srcdir)/SidePaneTreeStore.cs			\
	$(srcdir)/StringOps.cs				\
	$(srcdir)/Tri.cs 

BIBLIOGRAPHER_RESOURCES =					\
	-resource:$(top_srcdir)/resources/bibliographer.png	\
	-resource:$(top_srcdir)/resources/error.png		\
	-pkg:gnome-sharp-2.0					\
	-pkg:gtk-sharp-2.0					\
	-pkg:gtk-dotnet-2.0					\
	-r:/usr/lib/mono/2.0/Mono.Posix.dll			\
	-r:$(top_srcdir)/libbibby/libbibby.dll

$(WRAPPER): $(srcdir)/$(WRAPPER).in
	sed -e "s|\@prefix\@|$(prefix)|g"               \
	    -e "s|\@pkglibdir\@|$(pkglibdir)|g"         \
	    -e "s|\@target\@|$(TARGET)|g"               \
	    -e "s|\@mono\@|$(MONO)|g"                   \
	    -e "s|\@mono_flags\@|$(MONO_FLAGS)|g"       \
	    < $^ > $@

$(TARGET).config: $(srcdir)/$(TARGET).config.in
	sed -e "s|\@pkglibdir\@|$(pkglibdir)|" \
	< $^ > $@

$(TARGET): $(BIBLIOGRAPHER_CSFILES)
	$(CSC) -out:$@ $(BIBLIOGRAPHER_CSFILES) $(BIBLIOGRAPHER_RESOURCES) $(GTKSHARP_LIBS)

all: $(TARGET)

bibliographerdir = $(pkglibdir)
bibliographer_DATA = $(TARGET) $(TARGET).config

bin_SCRIPTS = $(WRAPPER)

EXTRA_DIST = $(BIBLIOGRAPHER_CSFILES) \
	$(top_srcdir)/gtk-gui/gui.stetic \
	bibliographer.in bibliographer.exe.config.in

CLEANFILES = 					\
	$(TARGET)				\
	$(TARGET).config			\
	$(WRAPPER)

MAINTAINERCLEANFILES = \
	Makefile.in
