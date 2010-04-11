ASSEMBLY_NAME = libbibby
ASSEMBLY = $(ASSEMBLY_NAME).dll
TARGET = $(ASSEMBLY)

CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS)

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
  	$(srcdir)/Debug.cs				\
	$(srcdir)/IntArrayList.cs			\
	$(srcdir)/StringArrayList.cs

RESOURCES =							\
	-resource:$(srcdir)/resources/bibtex_records	\
	-resource:$(srcdir)/resources/bibtex_fields 

$(ASSEMBLY): $(SOURCES)
	$(CSC) -target:library $(LIBFLAGS) -out:$@ $(SOURCES) -r:System $(RESOURCES)

all: $(ASSEMBLY)

EXTRA_DIST = $(SOURCES)

CLEANFILES = $(ASSEMBLY)

libbibbydir = $(pkglibdir)
libbibby_DATA = $(TARGET)

