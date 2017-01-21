using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBabel;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using LeapingChemistry;

namespace OBImport
{

    class Program
    {

        const string help =
@"  Usage {0} <inputfile> <outputfile>

        inputfile:  a file to read lesson data from.
        outputfile: a file in which to store the converted data.

    The intended use of this file is as a companion to the 'MolyPoly2' unity project
    for the purpose of generating molecule data without having to include OpenBabel
    as a runtime dependency.

    This program reads a list of lessons from a file. Each lesson is of the form:
        #optional comments may begin with a hash.
        <title>
        <description>
        <SMILES>
    
    Using OpenBabel, the SMILES string is parsed into a molecule specification.
    3-D geometry is then created for each molecule. (This may take some time if the molecule is large.)
    The original data plus the derived positional data is written to the output file.";

        static void Main(string[] args)
        {
 
            if(args.Length < 2 || args[0].Contains("help")) {
                Console.WriteLine(string.Format(help, "LessonPlanner"));
                return;
            }

            string inputfile = args[0];
            string outputfile = args[1];

            try
            {
                /* Open the input file and create the lesson plan */
                var lessonPlan = loadInputFile(inputfile);

                LessonSpec.writeXmlFile(outputfile, lessonPlan);

            /*    List<MoleculeSpec> molecules = new List<MoleculeSpec>();
                // for each LessonSpec, build the corresponding molecule data 
                foreach (var lesson in lessonPlan)
                {
                    molecules.Add(buildMoleculeData(lesson.smiles));
                }

                //Write each molecule to disk
                var moleculeWriter = new XmlSerializer(typeof(MoleculeSpec));
                foreach (var molecule in molecules)
                {
                    var name = getHash(molecule.smiles);
                    using (var file = new StreamWriter(name + ".xml"))
                    {
                        moleculeWriter.Serialize(file, molecule);
                    }
                }*/
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("File not found: " + e.FileName);
            }
        }

        static IEnumerable<LessonSpec> loadInputFile(string filename) {
			var data = new List<LessonSpec> ();
			var regex = new Regex (@"^(?!#)(?<title>.+)$\n^(?!#)(?<description>.+)$\n(?!#)(?<data>.+)$",
			                       	RegexOptions.ExplicitCapture | RegexOptions.Multiline);
			DoForEachMatch (filename, regex, match => {
				var lesson = new LessonSpec{
					title = match.Groups["title"].Value,
					description = match.Groups["description"].Value,
					//smiles = match.Groups["data"].Value,
                    molecule = buildMoleculeData(match.Groups["data"].Value),
				};

				data.Add(lesson);
			});
		
			return data;
		}

        public static void DoForEachMatch(string filename, Regex regex, Action<Match> action)
        {
            var stream = File.OpenText(filename);
            var matches = regex.Matches(stream.ReadToEnd());

            foreach (Match match in matches)
            {
                action(match);
            }
            stream.Close();
         
        }

        static MoleculeSpec buildMoleculeData(string smiles)
        {
            var conv        = new OBConversion();
            var mol         = new OBMol();
            var builder     = new OBBuilder();

            if (!conv.SetInFormat("smi"))
            {
                throw new NotSupportedException("smiles format not supported");
            }

            if (!conv.ReadString(mol, smiles))
            {
                throw new ArgumentException("could not read the smiles string: " + smiles);
            }

            mol.AddHydrogens();
            builder.Build(mol);
            mol.Center();

            var molecule = new MoleculeSpec() {
                atoms = new List<AtomSpec>(),
                bonds = new List<BondSpec>(),
                smiles = smiles,
            };
            
            foreach (var atom in mol.Atoms())
            {
                molecule.atoms.Add(ReadOBAtom(atom));
            }

            foreach(var bond in mol.Bonds())
            {
                molecule.bonds.Add(ReadOBBond(bond));
            }

            return molecule;
        }

        static AtomSpec ReadOBAtom(OBAtom atom)
        {
            var result = new AtomSpec()
            {
                AtomicNum = atom.GetAtomicNum(),
                AtomID = atom.GetId(),
                position = new Vector3()
                {
                    x = (float)atom.GetX(),
                    y = (float)atom.GetY(),
                    z = (float)atom.GetZ(),
                },
            };
            return result;
        }

        static BondSpec ReadOBBond(OBBond bond)
        {
            BondSpec.BondType type;
            if (bond.IsSingle())        type = BondSpec.BondType.Single;
            else if (bond.IsDouble())   type = BondSpec.BondType.Double;
            else                        type = BondSpec.BondType.Triple;
                   
            var result = new BondSpec()
            {
                firstAtomID = bond.GetBeginAtom().GetId(),
                secondAtomID = bond.GetEndAtom().GetId(),
                type = type,
            };
            return result;
        }

    }
}
