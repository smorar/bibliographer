ASSEMBLY_NAME = libbibby
ASSEMBLY = $(ASSEMBLY_NAME).dll
TARGET = $(ASSEMBLY)

CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS) $(NUNIT_LIBS)

if ENABLE_TESTS
ASSEMBLY_TEST = $(ASSEMBLY_NAME).test.dll

SOURCES_TEST = 					\
	$(srcdir)/tests/BibtexRecordTest.cs
endif

SOURCES = 						\
	$(srcdir)/AssemblyInfo.cs			\
	$(srcdir)/BibtexCustomData.cs			\
	$(srcdir)/BibtexCustomDataFields.cs		\
	$(srcdir)/BibtexCustomRecordField.cs		\
	$(srcdir)/BibtexRecord.cs			\
	$(srcdir)/BibtexRecordField.cs			\
	$(srcdir)/BibtexRecordFieldType.cs		\
	$(srcdir)/BibtexRecordFieldTypeLibrary.cs	\
	$(srcdir)/BibtexRecords.cs			\
	$(srcdir)/BibtexRecordType.cs			\
	$(srcdir)/BibtexRecordTypeLibrary.cs		\
    $(srcdir)/DatabaseStore.cs      \
    $(srcdir)/DatabaseStoreStatic.cs    \
	$(srcdir)/Debug.cs				\
	$(srcdir)/IntArrayList.cs			\
	$(srcdir)/StringArrayList.cs

RESOURCES =							\
	-resource:$(srcdir)/resources/bibtex_records	\
	-resource:$(srcdir)/resources/bibtex_fields

$(ASSEMBLY): $(SOURCES)
	$(CSC) -target:library $(LIBFLAGS) -out:$@ $(SOURCES) -r:System $(RESOURCES)

if ENABLE_TESTS

$(ASSEMBLY_TEST): $(SOURCES_TEST)
	$(CSC) -target:library $(LIBFLAGS) -out:$@ $(SOURCES_TEST) -r:System -r:$(ASSEMBLY) $(RESOURCES) $(NUNIT_LIBS)

all: $(ASSEMBLY) $(ASSEMBLY_TEST)

test: all
	$(NUNIT) $(ASSEMBLY_TEST)

EXTRA_DIST = $(SOURCES) $(SOURCES_TEST)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_TEST)

else

all: $(ASSEMBLY)

EXTRA_DIST = $(SOURCES)

CLEANFILES = $(ASSEMBLY)

endif

check-syntax:
	$(MAKE) syntax-target SYNTAX="-fsyntax-only"

syntax-target: $(ASSEMBLY)

libbibbydir = $(pkglibdir)
libbibby_DATA = $(TARGET)

