mergeInto(LibraryManager.library,
{
    SaveToLocalStorage: function (key, value)
    {
        localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
    },
    LoadFromLocalStorage: function (key)
    {
        var returnStr = localStorage.getItem(UTF8ToString(key));
        if (!returnStr)
        {
            return null;
        }
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },
    RemoveFromLocalStorage: function (key)
    {
        localStorage.removeItem(UTF8ToString(key));
    }
});
