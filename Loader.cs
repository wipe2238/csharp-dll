using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

internal sealed partial class DllExtension
{
    private static List<DllExtension> Extensions = new List<DllExtension>();

    public static uint LoadPattern( string pattern, string rootName )
    {
        uint loaded = 0;

        string[] files = Directory.GetFiles( Directory.GetParent( Assembly.GetExecutingAssembly().Location ).ToString(), pattern, SearchOption.TopDirectoryOnly );
        foreach( string file in files )
        {
            if( Load( Path.GetFileName( file ), rootName ) )
                loaded++;
        }

        return (loaded);
    }

    /// <summary>
    /// Attempt to load specified .dll file
    /// <param name="filename">.dll filename</param>
    /// <returns>True if .dll file has been loaded and passed all checks, false otherwise</returns>
    /// </summary>
    public static bool Load( string filename, string rootName )
    {
        _lastError = null;

        string file = Directory.GetParent( Assembly.GetExecutingAssembly().Location ).ToString();
        file += "\\" + filename;

        foreach( DllExtension dll in Extensions )
        {
            if( dll.Filename == file )
                return (false);
        }

        Assembly assembly = null;
        try
        {
            assembly = Assembly.LoadFile( file );
        }
        catch( Exception e )
        {
            _lastError = "Error loading assembly : " + e.Message;
            return (false);
        }

        if( assembly == null )
            return (false); // don't report errors

        Type root = null;

        string objName = rootName + "::" + rootName;

        try
        {
            root = assembly.GetType( objName.Replace( "::", "." ) );
        }
        catch( Exception )
        {
            _lastError = "Error getting root class";
            return (false);
        }

        if( root == null )
        {
            _lastError = objName + " : not found";
            return (false);
        }
        // public?
        else if( !root.IsPublic )
        {
            _lastError = objName + " : not public";
            return (false);
        }
        // class?
        else if( !root.IsClass )
        {
            _lastError = objName + " : not a class";
            return (false);
        }

        DllExtension extension = new DllExtension( root, rootName, filename );
        Extensions.Add( extension );

        return (true);
    }
}