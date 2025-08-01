var LibraryJsCalls = {
	Send: function(strObj,id, data64, fileNamePtr){
	      
	      var fileName = UTF8ToString(fileNamePtr);

	      window.SendEmail(Pointer_stringify(strObj),Pointer_stringify(id), Pointer_stringify(data64), fileName);
      },
};
mergeInto(LibraryManager.library, LibraryJsCalls);
