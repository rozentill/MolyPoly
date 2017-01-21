using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LeapingChemistry
{
    [Serializable()]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable()]
    public struct AtomSpec
    {
        public uint AtomID;
        public uint AtomicNum;
        public Vector3 position;
    }

    [Serializable()]
    public struct BondSpec
    {
        public uint firstAtomID;
        public uint secondAtomID;
    }

    [Serializable()]
    public struct MoleculeSpec
    {
        public string smiles;
        public List<AtomSpec> atoms;
        public List<BondSpec> bonds;
    }

    [Serializable()]
    public struct LessonSpec
    {
        public string title;
        public string description;
        public MoleculeSpec molecule;

        public static List<LessonSpec> readXmlFile(string filename)
        {
            var reader = new XmlSerializer(typeof(List<LessonSpec>));
            List<LessonSpec> data;

            using (var file = new StreamReader(filename))
            {
               data = (List<LessonSpec>) reader.Deserialize(file);
            }

            return data;
        }

        public static void writeXmlFile(string filename, IEnumerable<LessonSpec> lessonPlan)
        {
            /*Write the LessonPlan to disk*/
            var writer = new XmlSerializer(typeof(List<LessonSpec>));
            using (var file = new StreamWriter(filename))
            {
                writer.Serialize(file, lessonPlan);
            }
        }
    }

}
