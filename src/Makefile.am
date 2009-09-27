CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS)

TARGET = bibliographer.exe
WRAPPER = bibliographer

BIBLIOGRAPHER_CSFILES = 				\
	$(srcdir)/AboutBox.cs				\
	$(srcdir)/AssemblyInfo.cs			\
	$(srcdir)/BibliographerStartup.cs		\
	$(srcdir)/BibliographerUI.cs			\
	$(srcdir)/BibtexCustomRecordField.cs		\
	$(srcdir)/BibtexRecord.cs			\
	$(srcdir)/BibtexRecords.cs			\
	$(srcdir)/BibtexRecordField.cs			\
	$(srcdir)/IntArrayList.cs			\
	$(srcdir)/StringArrayList.cs			\
	$(srcdir)/BibtexRecordType.cs			\
	$(srcdir)/RunOnMainThread.cs		\
	$(srcdir)/BibtexRecordFieldType.cs		\
	$(srcdir)/BibtexRecordTypeLibrary.cs		\
	$(srcdir)/BibtexRecordFieldTypeLibrary.cs	\
	$(srcdir)/SidePaneTreeStore.cs			\
	$(srcdir)/LitListStore.cs			\
	$(srcdir)/FileIndexer.cs			\
	$(srcdir)/Tri.cs                                \
	$(srcdir)/Config.cs				\
	$(srcdir)/Cache.cs				\
  $(srcdir)/Debug.cs

BIBLIOGRAPHER_RESOURCES =					\
	-resource:$(top_srcdir)/resources/gui.glade		\
	-resource:$(top_srcdir)/resources/bibliographer.png	\
	-resource:$(top_srcdir)/resources/bibliographer_records	\
	-resource:$(top_srcdir)/resources/bibliographer_fields  \
	-resource:$(top_srcdir)/resources/error.png		\
	-pkg:glade-sharp-2.0					\
	-pkg:gnome-sharp-2.0					\
	-pkg:gtk-sharp-2.0					\
	-pkg:gtk-dotnet-2.0					\
	-r:/usr/lib/mono/2.0/Mono.Posix.dll

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

CLEANFILES = 					\
	$(TARGET)				\
	$(TARGET).config			\
	$(WRAPPER)

