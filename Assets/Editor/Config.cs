using System.Xml.Serialization;
using System.IO;
using System;

/// <summary>
/// Serializable class for the configuration
/// </summary>
[Serializable]
public class Config
{

    private string filePath = Path.Combine(Directory.GetCurrentDirectory() + @"\Assets\Editor\config.xml");

    /// <summary>
    /// automatically save the file after changes
    /// </summary>
    [XmlIgnore]
    public bool AutoSave { get; set; }

    /// <summary>
    /// Path to the containing file
    /// </summary>
    [XmlIgnore]
    public string FilePath
    {
        get { return filePath; }
        set 
        { 
            filePath = value;
            this.Save();
        }
    }


    /// <summary>
    /// Path to IFC convert
    /// </summary>
    public string IfcConvertPath 
    { 
        get
        {
            return this.ifcConvertPath;
        }

        set
        {
            this.ifcConvertPath = value;
            this.Save();
        }
    } 

    private string ifcConvertPath = Path.Combine(Directory.GetCurrentDirectory() + @"\Assets\Editor\IfcConvert\IfcConvert-x64.exe");

    /// <summary>
    /// Directory for the output files
    /// </summary>
    public string OutputPath 
    { 
        get
        {
            return this.outputPath;
        }

        set
        {
            this.outputPath = value;
            this.Save();
        }
    } 

    private string outputPath = Path.Combine(Directory.GetCurrentDirectory() + @"\Assets\Meshes");

    private static XmlSerializer serializer;

    /// <summary>
    /// static constructor to initialize the serializer
    /// </summary>
    static Config()
    {
        serializer = new XmlSerializer(typeof(Config));
    }

    /// <summary>
    /// Constructor for the deserialization
    /// </summary>
    internal Config() : this(false) { }

    /// <summary>
    /// public constructor
    /// </summary>
    /// <param name="autosave"></param>
    public Config(bool autosave = false)
    {
        this.AutoSave = autosave;
    }

    public void Serialize (Stream outputStream)
    {
        serializer.Serialize(outputStream, this);
        outputStream.Flush();
    }

    /// <summary>
    /// write this config object into the provided file path
    /// </summary>
    /// <param name="filePath"></param>
    public void Serialize (string filePath)
    {
        using (FileStream stream = File.Create(filePath))
        {
            this.Serialize(stream);
        }
    }

    /// <summary>
    /// generate a new config object from  a stream
    /// </summary>
    /// <param name="objectStream"></param>
    /// <returns></returns>
    public static Config Deserialize (Stream objectStream)
    {
        Config result = (Config)serializer.Deserialize(objectStream);
        result.AutoSave = true;
        return result;
    }

    /// <summary>
    /// Generate a new config object from a file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Config Deserialize (string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            return Deserialize(stream);
        }
    }

    /// <summary>
    /// automatically save the configuration
    /// </summary>
    private void Save()
    {
        if (this.AutoSave)
        {
            this.Serialize(this.FilePath); 
        }
    }
}
