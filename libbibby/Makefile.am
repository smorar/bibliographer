MCS_FLAGS += -nowarn:0169
MCS_FLAGS += -unsafe+

ASSEMBLY = libbibby
TARGET = library
LINK = $(REF_LIBBIBBY)

LIBBIBBY_CSFILES = 				\
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

LIBBIBBY_RESOURCES =					\
	-resource:$(top_srcdir)/resources/bibtex_records	\
	-resource:$(top_srcdir)/resources/bibtex_fields 

MAINTAINERCLEANFILES = \
	Makefile.in
