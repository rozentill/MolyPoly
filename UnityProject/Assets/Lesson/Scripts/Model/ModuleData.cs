using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ModuleData {


	public class Module
	{
		public readonly string name;
		public Chemistry.Element[] atoms;
		public Lesson[] lessons;

		public Module(string name) {
			this.name = name;
		}
	}

	public class Lesson {
		public string name;
		
		public delegate Maybe<string> CheckMolecule (IEnumerable<EditAtom> listAtoms);
		public List<CheckMolecule> checkers;

		public Maybe<string> run(IEnumerable<EditAtom> listAtoms) {
			foreach (var rule in checkers) {
				var result = rule(listAtoms);
				if(result.any) return result;
			}
			return new None<string> ();
		}
	}


	public class MoleculeChecker
	{
		public static MoleculeChecker start {
			get {
				return new MoleculeChecker();
			}
		}

		private MoleculeChecker() {
			checkers = new List<Lesson.CheckMolecule>();
		}

		/* Run through the list of molecule checking rules.
		 * If any return a string, stop and return that result as a hint to the user.
		 * If all checkers do not return a string, that means the molecule is "correct"
		 */
		public Maybe<string> run(IEnumerable<EditAtom> m) {
			var results = new List<string> ();
			foreach (var rule in checkers) {
				var next = rule(m);
				if(next.any) {
					results.Add(next.value);
				}
			}

			if (results.Any())
								return new Just<string> (results.Aggregate ((a, b) => a + "\n" + b));
			else 
								return new None<string> ();
		}

		public static Maybe<string> CountElement(IEnumerable<EditAtom> atoms, Chemistry.Element element, int count) {
			if(	atoms.Count (atom => atom.elem == element) != count) {
				var message = string.Format ("There should be {1} {0} atoms.", ColouredBall.GetDisplay((int)element).name, count);
				return new Just<string>(message);
			}
			return new None<string>();
		}

		public static bool ConnectedTest(EditAtom a, Chemistry.Element elem, ConnectedSpec[] specs) {
			return a.elem == elem && specs.All (spec => {
				return a.neighbours.Count(neighbour => neighbour.elem == spec.element) == spec.count;
			});
		}

		public static bool ConnectedWithBondTest(EditAtom a, Chemistry.Element elem, ConnectedSpec[] specs) {
			return a.elem == elem && specs.All (spec => {
			//	return a.neighbours.Count(neighbour => neighbour.elem == spec.element) == spec.count;
				return a.neighbourWithDegree.Count (neighbour => neighbour.target.elem == spec.element && (spec.bond_degree == 0 || neighbour.degree == spec.bond_degree)) == spec.count;
			});
		}

		public struct ConnectedSpec
		{
			public Chemistry.Element element;
			public int bond_degree;
			public int count;
		}

		public MoleculeChecker CountElement(Chemistry.Element element, int count) {
			checkers.Add ((molecule) => MoleculeChecker.CountElement(molecule, element, count));
			return this;
		}

		public static string GetBondName(int degree) {
			string[] names = new string[] {"", "single", "double", "triple"};
			return names [degree];
		}

		private static string ConnectedSpecDescription(ConnectedSpec[] specs) {
			return specs.Select (spec => string.Format ("{0} {1}{2}", spec.count, spec.element, 
			                                   spec.bond_degree > 0 ? string.Format (" (with a {0} bond)", GetBondName (spec.bond_degree)) : ""))
				.Aggregate((a, b) => string.Format ("{0} and {1}", a, b));
		}

		public MoleculeChecker CountConnectedTo(Chemistry.Element element, int count, ConnectedSpec[] specs) {
			checkers.Add ((atoms) => {
				if(atoms.Count(atom => ConnectedWithBondTest(atom, element, specs)) != count) {
					var message = string.Format("There should be {1} {0} atoms, each connected to {2}", ColouredBall.GetDisplay((int)element).name, count,
					                            ConnectedSpecDescription(specs));
					return new Just<string>(message);
				}
				else
					return new None<string>();
			});
			return this;
		}

		public MoleculeChecker CountRecursiveConnectedTo(Chemistry.Element element, int count, ConnectedSpec[] specs, Chemistry.Element recursiveElement, ConnectedSpec recursive) {
			checkers.Add ((atoms) => {
				if(atoms.Count(atom => ConnectedWithBondTest(atom, element, specs)
				       && atom.neighbours.Count(neighbour => ConnectedWithBondTest(neighbour, recursiveElement, new ConnectedSpec[]{recursive})) == 0) != count) {
					var message = string.Format ("The <b>{1} {0}</b>\n that is connected to <b>{2}</b>\nshould not be connected to any <b>{3}</b>\n which is connected to <b>{4}</b>", 
					                             ColouredBall.GetDisplay((int)element).name, count,
					                             ConnectedSpecDescription(specs),
					                             ColouredBall.GetDisplay((int)recursiveElement).name,
					                             ConnectedSpecDescription(new ConnectedSpec[]{recursive}));
					return new Just<string>(message);
				}
				else
					return new None<string>();
			});
			return this;
		}
	
		private List<Lesson.CheckMolecule> checkers;
	}
	
	public static readonly Dictionary<string, Module> modules;

	static ModuleData() {
		modules = new Dictionary<string, Module> ();

		System.Action<string, Chemistry.Element[], Lesson[]> AddModule = delegate(string name, Chemistry.Element[] atoms, Lesson[] lessons) {
			var m = new Module(name) {
				atoms = atoms,
				lessons = lessons,
			};
			modules.Add(name, m);
		};



		AddModule("Alkane", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen},
		   new Lesson[] {
			new Lesson() {
				name = "ethane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 2).CountElement(Chemistry.Element.Hydrogen, 6).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
																new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
																new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
															}).run,


				},
			},
			new Lesson() {
				name = "2-methylpropane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 10).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 3,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
					}).run,
				},
			},
			new Lesson() {
				name = "butane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 10).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
				},
			},
		});



		AddModule("Alkene", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen},
		new Lesson[] {
			new Lesson() {
				name = "ethene",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 2).CountElement(Chemistry.Element.Hydrogen, 4).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 2},
					}).run,
				},
			},
			new Lesson() {
				name = "2,3-dimethylbut-2-ene",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 6).CountElement(Chemistry.Element.Hydrogen, 12).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 4,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 2},
					}).run,
				},
			},
			new Lesson() {
				name = "2-methylopropene",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 8).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 2},
					}).run,
				},
			},
		});

		AddModule("Alkyne", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen},
		new Lesson[] {
			new Lesson() {
				name = "ethyne",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 2).CountElement(Chemistry.Element.Hydrogen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 3},
					}).run,
				}
			},
			new Lesson() {
				name = "But-1-yne",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 6).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 3},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
				},
			},
			new Lesson() {
				name = "propyne",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 3).CountElement(Chemistry.Element.Hydrogen, 4).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1, bond_degree = 3},
					}).run,
				},
			},
		});
		
		AddModule("Ether", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "methoxymethane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 2)
					.CountElement(Chemistry.Element.Hydrogen, 6)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "2-ethoxypropane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 5)
					.CountElement(Chemistry.Element.Hydrogen, 12)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "methoxymethane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 8)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
				}
			},
		});
	
		
		AddModule("Alcohol", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "methanol",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 1)
					.CountElement(Chemistry.Element.Hydrogen, 4)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Oxygen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "Propan-1,3 diol",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 8)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Oxygen, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "Propan-2-ol",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 8)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Oxygen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
				}
			},
		});

		AddModule("Ketone", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "propanone",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 6)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "2-methylpentan-3-one",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
						.CountElement(Chemistry.Element.Carbon, 6)
						.CountElement(Chemistry.Element.Hydrogen, 12)
						.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 3,
					new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountRecursiveConnectedTo(Chemistry.Element.Carbon, 1,
					new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					},
					Chemistry.Element.Carbon,
					new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3}).run,
				}
			},
			new Lesson() {
				name = "Pentan-3-one",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 5)
					.CountElement(Chemistry.Element.Hydrogen, 10)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
		});

		AddModule("Aldehyde", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "methal",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 1)
					.CountElement(Chemistry.Element.Hydrogen, 2)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "2-ethylbutanal",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 6)
					.CountElement(Chemistry.Element.Hydrogen, 12)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
					MoleculeChecker.start.CountRecursiveConnectedTo(Chemistry.Element.Carbon, 1,
					                                                new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
					},
					Chemistry.Element.Carbon,
					new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3}).run,
				}
			},
			new Lesson() {
				name = "propanal",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 6)
					.CountElement(Chemistry.Element.Oxygen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
		});

		AddModule("Ester", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "methyl methanoate",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 2)
					.CountElement(Chemistry.Element.Hydrogen, 4)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "ethyl propanoate",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 5)
					.CountElement(Chemistry.Element.Hydrogen, 10)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "methyl ethanoate",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 6)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
		});


		AddModule("Carboxylic Acid", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "ethanoic acid",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 2)
					.CountElement(Chemistry.Element.Hydrogen, 4)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Oxygen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "3-methylbutanoic acid",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 5)
					.CountElement(Chemistry.Element.Hydrogen, 10)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "propanoic acid",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 6)
					.CountElement(Chemistry.Element.Oxygen, 2).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Oxygen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
		});

		AddModule("Amine", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Nitrogen},
		new Lesson[] {
			new Lesson() {
				name = "methanamine",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 1)
					.CountElement(Chemistry.Element.Hydrogen, 5)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "N, N-diethylethanamine",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 6)
					.CountElement(Chemistry.Element.Hydrogen, 15)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 3,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 3,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1}
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
					}).run,
				}
			},
			new Lesson() {
				name = "propanamine",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 9)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,

				}
			},
		});

		AddModule("Amide", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen, Chemistry.Element.Nitrogen,Chemistry.Element.Oxygen},
		new Lesson[] {
			new Lesson() {
				name = "ethanamide",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 2)
					.CountElement(Chemistry.Element.Hydrogen, 5)
					.CountElement(Chemistry.Element.Oxygen, 1)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
				}
			},
			new Lesson() {
				name = "N-methylpropanamide",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 4)
					.CountElement(Chemistry.Element.Hydrogen, 9)
					.CountElement(Chemistry.Element.Oxygen, 1)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1}
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						//new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1},
					}).run,
				}
			},
			new Lesson() {
				name = "propanamide",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start
					.CountElement(Chemistry.Element.Carbon, 3)
					.CountElement(Chemistry.Element.Hydrogen, 7)
					.CountElement(Chemistry.Element.Oxygen, 1)
					.CountElement(Chemistry.Element.Nitrogen, 1).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Nitrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Oxygen, count = 1, bond_degree = 2},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Nitrogen, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,

				}
			},
		});

		AddModule("FreeStyle", new Chemistry.Element[] {Chemistry.Element.Carbon, Chemistry.Element.Hydrogen},
		new Lesson[] {
			// all dummies Lesson
			new Lesson() {
				name = "free style",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 2).CountElement(Chemistry.Element.Hydrogen, 6).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					
					
				},
			},
			new Lesson() {
				name = "2-methylpropane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 10).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 3,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 1,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 1},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 3},
					}).run,
				},
			},
			new Lesson() {
				name = "butane",
				checkers = new List<Lesson.CheckMolecule>() {
					MoleculeChecker.start.CountElement(Chemistry.Element.Carbon, 4).CountElement(Chemistry.Element.Hydrogen, 10).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 3},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 1},
					}).run,
					MoleculeChecker.start.CountConnectedTo(Chemistry.Element.Carbon, 2,
					                                       new MoleculeChecker.ConnectedSpec[] {
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Hydrogen, count = 2},
						new MoleculeChecker.ConnectedSpec() {element = Chemistry.Element.Carbon, count = 2},
					}).run,
				},
			},
		});
	
//asdasdasdadasdadasdas

	}
}