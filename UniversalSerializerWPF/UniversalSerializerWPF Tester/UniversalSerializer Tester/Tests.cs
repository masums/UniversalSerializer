﻿
// Copyright Christophe Bertrand.

#if SILVERLIGHT
#define LIMITED_RIGHTS_SILVERLIGHT // Mainly tests using private members of framework types.
#endif

//#define DEBUG_WriteSerializedFile

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !SILVERLIGHT && !NETFX_CORE
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap; // in System.Runtime.Serialization.Formatters.Soap.dll.
using System.Web.Script.Serialization; // in System.Web.Extensions.dll .
#endif
using System.Windows;
#if NETFX_CORE // Windows Store 8.
using Windows.UI.Xaml;
using Windows.UI.Text;
using UniversalSerializer_Windows_Store_8_PCL;
#else
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TestTypes;
#endif
using System.Windows.Input;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using UniversalSerializerLib3;
using UniversalSerializerResourceTests;

namespace Test_UniversalSerializer
{

	public static class Tests
	{

		// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
		const SerializerFormatters TestSerFormatter = SerializerFormatters.BinarySerializationFormatter;
		// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

		static bool AutomaticTests = true;




		// ===============================================================================

		public static void RunTests(
#if SILVERLIGHT || NETFX_CORE
MainPage
#else
Window
#endif
 window)
		{

			SimpleTests(window);

			ExternalCustomContainerTest();

			ShareTest();

#if !SILVERLIGHT
			XmlFormatterTest();
#endif

			CustomFiltersTest();

			CustomContainerTest();

			XmlFile2_0Test();
			JsonFile2_0Test();
			BinaryFile2_0Test();

			DifficultiesTests();

		}

		// ===============================================================================

		public static void XmlFormatterTest()
		{
			string s = "Hello!";
			using (MemoryStream fs = new MemoryStream())
			{
				Parameters parameters = new Parameters() { Stream = fs, SerializerFormatter = SerializerFormatters.XmlSerializationFormatter };
				UniversalSerializer ser = new UniversalSerializer(parameters);

				var data = new MyObjectIDictionary();
				data.Add(123, s);

				ser.Serialize(data);

				var o = ser.Deserialize<MyObjectIDictionary>();
				bool ok = ((string)o[123]) == s;
				Debugger.Break();
			}

		}

		// ===============================================================================

		/// <summary>
		/// Test compatibility reading serialized data as version 2.0.
		/// </summary>
		public static void BinaryFile2_0Test()
		{
#if !SILVERLIGHT && !NETFX_CORE
			using (FileStream fs = new FileStream(@"TestFiles\Serialized2.0.data", FileMode.Open))
			{
				Parameters parameters = new Parameters() { Stream = fs, SerializerFormatter = SerializerFormatters.BinarySerializationFormatter };
				UniversalSerializer ser = new UniversalSerializer(parameters);

				var o = ser.Deserialize();
				bool ok = o.GetType() == typeof(MyByteColorArray.MyByteColor[]);
				Debugger.Break();
			}
#endif
		}

		// ===============================================================================

		/// <summary>
		/// Test compatibility reading serialized data as version 2.0.
		/// </summary>
		public static void XmlFile2_0Test()
		{
#if !SILVERLIGHT && !NETFX_CORE
			using (FileStream fs = new FileStream(@"TestFiles\Serialized2.0.xml", FileMode.Open))
			{
				Parameters parameters = new Parameters() { Stream = fs, SerializerFormatter = SerializerFormatters.XmlSerializationFormatter };
				UniversalSerializer ser = new UniversalSerializer(parameters);

				var o = ser.Deserialize();
				bool ok = o.GetType() == typeof(MyByteColorArray.MyByteColor[]);
				Debugger.Break();
			}
#endif
		}

		// ===============================================================================

		/// <summary>
		/// Test compatibility reading serialized data as version 2.0.
		/// </summary>
		public static void JsonFile2_0Test()
		{
#if !PORTABLE && !SILVERLIGHT && !NETFX_CORE
			using (FileStream fs = new FileStream(@"TestFiles\Serialized2.0.json", FileMode.Open))
			{
				Parameters parameters = new Parameters() { Stream = fs, SerializerFormatter = SerializerFormatters.JSONSerializationFormatter };
				UniversalSerializer ser = new UniversalSerializer(parameters);

				var o = ser.Deserialize();
				bool ok = o.GetType() == typeof(MyByteColorArray.MyByteColor[]);
				Debugger.Break();
			}
#endif
		}

		// ===============================================================================

		/// <summary>
		/// Tests that generate errors or exceptions.
		/// </summary>
		static void DifficultiesTests()
		{
			Dictionary<string, bool> ResultsLog = new Dictionary<string, bool>();

			// - - - -

			{
				string title = "This type is marked as Serializable, but is incompatible, and it has no exploitable constructor.";
				bool ok;
				string msg;
				var data = new NotCompatibleWithSerializableAttribute(new NotCompatibleWithSerializableAttribute.NotMarkedAsSerializable());
				try
				{
					var data2 = EasySerializeThenDeserialize<NotCompatibleWithSerializableAttribute>(data);
					ok = true;
					msg = "ok";
				}
				catch (Exception e)
				{
					ok = false;
					msg = e.Message;
				}

				ResultsLog.Add(title + " {" + data.GetType().GetName() + "} + Result = " + msg, ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


			// - - - -
			// - - - -
			// - - - -

			{
				List<string> Failed = new List<string>();
				bool AllOK =
					ResultsLog.All(p =>
					{
						if (!p.Value)
							Failed.Add(p.Key);
						return p.Value;
					});
				bool ok = !AllOK; // All tests should fail (that is the goal of this test serie).
				Debugger.Break();
			}

		}

		// ===============================================================================

		static void SimpleTests(
#if SILVERLIGHT || NETFX_CORE
MainPage
#else
Window
#endif
 window
						)
		{
			Dictionary<string, bool> ResultsLog = new Dictionary<string, bool>();
#if ! DEBUG
#error The application should be compiled in DEBUG mode.
			MessageBox.Show("The application should be compiled in DEBUG mode");
#endif


			// - - - -



			#region new tests

			{
				var descr = new MyByteColorArray();
				var R = EasySerializeThenDeserializeWithDescriptor(descr, 3, ResultsLog);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				var descr = new MyObjectContainerArray();
				var R = EasySerializeThenDeserializeWithDescriptor(descr, 3, ResultsLog);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				var descr = new PrimitiveTypesStructureArray();
				var R = EasySerializeThenDeserializeWithDescriptor(descr, 3, ResultsLog);
				if (!AutomaticTests)
					Debugger.Break();
			}



			#endregion new tests



			#region simple_tests

			{
				string title = "";
				var data = new ClassInheritingAPrivateField(2);
				var data2 = EasySerializeThenDeserialize<ClassInheritingAPrivateField>(data);
				bool ok = data2 != null
					&& data2.PrivateFieldGetterX2 == data.PrivateFieldGetterX2;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new List<double?>() { (double?)1.2, (double?)0.0 };
				var data2 = EasySerializeThenDeserialize<List<double?>>(data);
				bool ok = data2 != null
					&& data2[0].HasValue && data2[0].Value == 1.2
					&& data2[1].HasValue && data2[1].Value == 0.0;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Advanced circular types tests.";
				var data = new CircularTests();
				var data2 = EasySerializeThenDeserialize<CircularTests>(data);
				bool ok =
					data2 != null
					&& data2.TestIntegrity();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new CircularListInAField();
				data.List = new List<CircularListInAField>();
				data.List.Add(data);
				var data2 = EasySerializeThenDeserialize<CircularListInAField>(data);
				bool ok = data2 != null && data2.List.Count > 0 && data2.List[0] == data2;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new CircularDictionaryInAField();
				data.Dict = new Dictionary<int, CircularDictionaryInAField>();
				data.Dict.Add(1, data);
				var data2 = EasySerializeThenDeserialize<CircularDictionaryInAField>(data);
				bool ok = data2 != null && data2.Dict.ContainsKey(1) && data2.Dict[1] == data2;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if !SILVERLIGHT && !NETFX_CORE && !PORTABLE
			{
				string title = "TransformGroup, RotateTransform & TranslateTransform in a Line.";
				System.Windows.Shapes.Shape data = new System.Windows.Shapes.Line() { X1 = 0.0, Y1 = 0.0, X2 = 100.0, Y2 = 100.0 };

				var tfg = new System.Windows.Media.TransformGroup();
				var rotate = new System.Windows.Media.RotateTransform(3.0);
				var translate = new System.Windows.Media.TranslateTransform(0.0, 10.0);

				tfg.Children.Add(rotate);
				tfg.Children.Add(translate);
				data.RenderTransform = tfg;
				var data2 = EasySerializeThenDeserializeWPF<System.Windows.Shapes.Shape>(data);
				bool ok = data2 is System.Windows.Shapes.Line;
				if (ok)
				{
					var line2 = data2 as System.Windows.Shapes.Line;
					ok &= line2 != null && line2.Y2 == 100.0;
					if (ok)
					{
						var tfg2 = line2.RenderTransform as System.Windows.Media.TransformGroup;
						ok &= tfg2 != null && tfg2.Children.Count == 2;
						if (ok)
						{
							var rotate2 = tfg2.Children[0] as System.Windows.Media.RotateTransform;
							ok &= rotate2 != null && rotate2.Angle == 3.0;
							if (ok)
							{
								var translate2 = tfg2.Children[1] as System.Windows.Media.TranslateTransform;
								ok &= translate2 != null && translate2.Y == 10.0;
							}
						}
					}
				}
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "LineGeometry in a Path.";
				var data = new System.Windows.Shapes.Path() { Data = new System.Windows.Media.LineGeometry { StartPoint = new Point(0, 0), EndPoint = new Point(100, 100) } };
				var data2 = EasySerializeThenDeserializeWPF<System.Windows.Shapes.Path>(data);
				bool ok = data2 is System.Windows.Shapes.Path && data2.Data is System.Windows.Media.LineGeometry && ((System.Windows.Media.LineGeometry)data2.Data).EndPoint.Y == 100;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{
				string title = "ForceSerialize on a private field and property.";
				var data = new ForceSerializeOnPrivateFieldAndProperty(new double[2] { 5.0, 10.0 });
				var data2 = EasySerializeThenDeserialize<ForceSerializeOnPrivateFieldAndProperty>(data);
				bool ok = data[1] == data2[1];
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "This type contains itself.";
				var data = new CircularType() { Id = 1234 };
				data.SubItem = new CircularType() { Id = 5678 };
				var data2 = EasySerializeThenDeserialize<CircularType>(data);
				bool ok = data2.Id == data.Id && data2.SubItem.Id == data.SubItem.Id;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Circular type with generic list.";
				var data = new CircularTypeWithGenericList() { Name = "This name", Id = 1234 };
				data.others.Add(new CircularTypeWithGenericList() { Name = "Other name", Id = 5678 });
				var data2 = EasySerializeThenDeserialize<CircularTypeWithGenericList>(data);
				bool ok = data2.Name == data.Name && data2.others[0].Id == data.others[0].Id;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "DateTime Kind.";
				var data = new DateTime[2] { DateTime.Now, DateTime.UtcNow };
				var data2 = EasySerializeThenDeserialize<DateTime[]>(data);
				bool ok = data2[0].Ticks == data[0].Ticks && data2[0].Kind == data[0].Kind
					&& data2[1].Ticks == data[1].Ticks && data2[1].Kind == data[1].Kind;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new ParametricConstructedClassWithPublicField(new int[2] { 10, 20 });
				var data2 = EasySerializeThenDeserialize<ParametricConstructedClassWithPublicField>(data);
				bool ok = data2.Equals(data);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if !SILVERLIGHT && !NO_WPF_TESTS

			{
				string title = "";
				var data = new Thickness(10, 20, 30, 40);
				var data2 = EasySerializeThenDeserialize<Thickness>(data);
				bool ok = data2.Equals(data);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "no no-param constructor, so we need ParametricConstructorsManager. contains<UIElementCollection>";
				var data = new contains<UIElementCollection>(
										new System.Windows.Controls.UIElementCollection(
							new UIElement(), new FrameworkElement()));
				data.contained.Add(new TextBox());
				var data2 = EasySerializeThenDeserialize<contains<UIElementCollection>>(data);
				bool ok = data2.contained[0] is TextBox;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#endif

			{
				string title = "various integers.";
				var data = new Integers();
				var data2 = EasySerializeThenDeserialize<Integers>(data);
				bool ok = data2.Equals(data);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "All primitive types.";
				var data = new PrimitiveTypes();
				var data2 = EasySerializeThenDeserialize<PrimitiveTypes>(data);
				bool ok = data2.Equals(data);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "negative integers (binaryformatter compresses them).";
				var data = new Integers();
				data.Short = -1;
				data.Int = -2;
				data.Long = -3;
				var data2 = EasySerializeThenDeserialize<Integers>(data);
				bool ok = data2.Equals(data);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Array of structures in a box.";
				Box<MyColor[]> data = new Box<MyColor[]>();
				data.value = new MyColor[10];
				for (int i = 0; i < 10; i++)
					data.value[i] = new MyColor() { R = (byte)(i & 255), G = (byte)(i >> 8 & 255), B = (byte)(i >> 16 & 255) };
				// NEGATIVE NUMBERS: data.value[i] = new MyColor() { R = -(int)((byte)(i & 255)), G = -(int)((byte)(i >> 8 & 255)), B = -(int)((byte)(i >> 16 & 255)) };
				var data2 = EasySerializeThenDeserialize<Box<MyColor[]>>(data);
				bool ok = data2.value[1].R == 1;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if ! SILVERLIGHT
			{
				string title = "2 references of a string in an object property.";
				string s = "Hello";
				var data = new KeyValuePair<object, object>(s, s);
				bool sameReference = object.ReferenceEquals(data.Key, data.Value);
				var data2 = EasySerializeThenDeserialize<KeyValuePair<object, object>>(data);
				bool sameReference2 = object.ReferenceEquals(data2.Key, data2.Value);
				bool ok = sameReference2 && (string)data2.Key == "Hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !NO_WPF_TESTS
			// Under portable lib, causes a System.Security.VerificationException with System.Windows.Size, property Width.
			// Idem under Windows Phone 8.
			{
				string title = "A CLR structure.";
				var data = new System.Windows.Size(12.3, 45.6);
				var data2 = EasySerializeThenDeserialize<System.Windows.Size>(data);
				bool ok = data2.Width == 12.3 && data2.Height == 45.6;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !SILVERLIGHT
			{
				string title = "A private class with a private constructor.";
				var data = (PrivateClass)Activator.CreateInstance(typeof(PrivateClass), true);
				var data2 = EasySerializeThenDeserialize<PrivateClass>(data);
				bool ok = data2 is PrivateClass;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{
				string title = "Illegal characters in some formats.";
				// Should fail with xml since \b and \f are illegal characters in xml, we remove them.
				var data = "\\\b\t\n\f\r<>&\"'{},[]:Hôtel";
				var data2 = EasySerializeThenDeserialize<string>(data);
				bool ok = data2 == data;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Public enum";
				PublicEnum data = PublicEnum.Something;
				var data2 = EasySerializeThenDeserialize<PublicEnum>(data);
				bool ok = data2 == data;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Private enum";
				PrivateEnum data = PrivateEnum.Something;
				var data2 = EasySerializeThenDeserialize<PrivateEnum>(data);
				bool ok = data2 == data;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "one null string and one empty string";
				var data = new string[2] { null, string.Empty };
				var data2 = EasySerializeThenDeserialize<string[]>(data);
				bool ok = data2[0] == null && data2[1] == string.Empty;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				decimal data = 12345678901234567890m;
				var data2 = EasySerializeThenDeserialize<decimal>(data);
				bool ok = data2 == data;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "no no-param constructor, so we need ParametricConstructorsManager. contains<MyGenericICollectionWithoutNoParamConstructor>";
				var data = new contains<MyGenericICollectionWithoutNoParamConstructor>(
						new MyGenericICollectionWithoutNoParamConstructor(55));
				data.contained.Add(123);
				var data2 = EasySerializeThenDeserialize<contains<MyGenericICollectionWithoutNoParamConstructor>>(data);
				bool ok = data2.contained.Contains(123);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


			{
				string title = "MyGenericIDictionaryMoreProps";
				var data = new MyGenericIDictionaryMoreProps();
				data.Add(123, "hello");
				data.MyField = "To be serialized";
				var data2 = EasySerializeThenDeserialize<MyGenericIDictionaryMoreProps>(data);
				bool ok = data2[123] == "hello" && data2.MyField == "To be serialized";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if !NO_WPF_TESTS
			{
				string title = "";
				var data = new contains<Nullable<bool>>(true);
#if SILVERLIGHT
				var data2 = EasySerializeThenDeserialize<contains<Nullable<bool>>>(data);
#else
				var data2 = EasySerializeThenDeserializeWPF<contains<Nullable<bool>>>(data);
#endif
				bool ok = data2.contained.Value;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new contains<Nullable<double>>(132.456);
#if SILVERLIGHT
				var data2 = EasySerializeThenDeserialize<contains<Nullable<double>>>(data);
#else
				var data2 = EasySerializeThenDeserializeWPF<contains<Nullable<double>>>(data);
#endif
				bool ok = data2.contained.Value == 132.456;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if SILVERLIGHT
#if !LIMITED_RIGHTS_SILVERLIGHT
			// Still (2015-03-20) have a problem with "System.Windows.Input.CursorType", due to private member access.
			{
				string title = "Simple MainPage";
				var data = new MainPage();
				var data2 = EasySerializeThenDeserialize<MainPage>(data);
				bool ok = data2 != null;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif
#else
			{
				string title = "Simple WPF Window";
				var data = new DerivedWindow() { Title = "Dynamic Window" };
				var data2 = EasySerializeThenDeserializeWPF<Window>(data);
				bool ok = data2 is DerivedWindow && data2.Title == data.Title;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
				data.Close();
				data2.Close();
			}
#endif
#endif // !NO_WPF_TESTS


			{
				string title = "";
				var data = new List<object>();
				data.Add("hello");
				data.Add(123);
				data.Add(new contains<int>());
				var data2 = EasySerializeThenDeserialize<List<object>>(data);
				bool ok = (string)data2[0] == "hello" && (int)data2[1] == 123 && data2[2] is contains<int>;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if (!SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)) && !NO_WPF_TESTS
			// Still (2013-08-29) have a problem with "System.Windows.Input.CursorType".
			{
				string title = "Two references to the same object. in contains";
				var data = new contains<TextBox[]>(new TextBox[2]);
				data.contained[0] = new TextBox() { Text = "TextBox1" };
				data.contained[1] = data.contained[0]; // Same reference
				var data2 = EasySerializeThenDeserialize<contains<TextBox[]>>(data);
				data2.contained[0].Text = "New text";
				bool sameReference = object.ReferenceEquals(data2.contained[0], data2.contained[1]);
				bool ok = sameReference && data2.contained[1].Text == "New text";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{
				string title = "class dictionary.";
				var data = new Dictionary<int, VerySimpleClass>();
				data.Add(123, new VerySimpleClass() { a = 456 });
				var data2 = EasySerializeThenDeserialize<Dictionary<int, VerySimpleClass>>(data);
				bool ok = data2[123].a == 456;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


			{
				string title = "Two references to the same object.";
				var data = new VerySimpleClass[2];
				data[0] = new VerySimpleClass() { a = 123 };
				data[1] = data[0]; // Same reference
				var data2 = EasySerializeThenDeserialize<VerySimpleClass[]>(data);
				data2[0].a = 789;
				bool sameReference = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference && data2[1].a == 789;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "";
				var data = new ResourceDictionary();
				string key = "WindowFontStyle";
				data.Add(key, window.FontStyle);
				var data2 = EasySerializeThenDeserialize<ResourceDictionary>(data);
				bool ok = data2.Values.Count == 1 && data2[key] is FontStyle && data2[key].ToString() == data[key].ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}



			{
				string title = "strings with same value.";
				var data = new string[2];
				data[0] = "hello";
				data[1] = data[0];
				bool sameReference = object.ReferenceEquals(data[0], data[1]);
				var data2 = EasySerializeThenDeserialize<string[]>(data);
				bool sameReference2 = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference2 && data2[0] == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "strings in a string array.";
				var data = new string[2];
				data[0] = "hello";
				data[1] = data[0];
				bool sameReference = object.ReferenceEquals(data[0], data[1]);
				var data2 = EasySerializeThenDeserialize<string[]>(data);
				bool sameReference2 = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference2 && (string)data2[0] == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "strings in an object array.";
				var data = new object[2];
				data[0] = "hello";
				data[1] = data[0];
				bool sameReference = object.ReferenceEquals(data[0], data[1]);
				var data2 = EasySerializeThenDeserialize<object[]>(data);
				bool sameReference2 = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference2 && (string)data2[0] == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "strings in a contains<object array>.";
				var data = new contains<object[]>(new object[2]);
				data.contained[0] = "hello";
				data.contained[1] = data.contained[0];
				bool sameReference = object.ReferenceEquals(data.contained[0], data.contained[1]);
				var data2 = EasySerializeThenDeserialize<contains<object[]>>(data);
				bool sameReference2 = object.ReferenceEquals(data2.contained[0], data2.contained[1]);
				bool ok = sameReference2 && (string)data2.contained[0] == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "contains<Dictionary<string, object>>";
				var data = new contains<Dictionary<string, object>>(new Dictionary<string, object> { { "aaa", 123 }, { "bbb", "end" } });
				var data2 = EasySerializeThenDeserialize<contains<Dictionary<string, object>>>(data);
				bool ok = (int)data2.contained["aaa"] == 123 && (string)data2.contained["bbb"] == "end";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Dictionary<string, object>";
				var data = new Dictionary<string, object> { { "aaa", 123 }, { "bbb", "end" } };
				var data2 = EasySerializeThenDeserialize<Dictionary<string, object>>(data);
				bool ok = (int)data2["aaa"] == 123 && (string)data2["bbb"] == "end";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Cycle because a member contains a reference to a parent.";
				var data = new contains<object>();
				data.contained = data; // Cycle.
				var data2 = EasySerializeThenDeserialize<contains<object>>(data);
				bool ok = data2.contained == data2;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "Two references to the same VerySimpleClass.";
				var data = new VerySimpleClass[2];
				data[0] = new VerySimpleClass() { a = 123 };
				data[1] = data[0]; // Same reference
				var data2 = EasySerializeThenDeserialize<VerySimpleClass[]>(data);
				data2[0].a = 789;
				bool sameReference = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference && data2[1].a == 789;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "GenericClassFromGenericIntListWithOtherType<string>";
				var data = new GenericClassFromGenericIntListWithOtherType<string>();
				data.Add(123);
				data.Value = "hello";
				var data2 = EasySerializeThenDeserialize<GenericClassFromGenericIntListWithOtherType<string>>(data);
				bool ok = data2.Value == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "contains<int>";
				var data = new contains<int>(123);
				var data2 = EasySerializeThenDeserialize<contains<int>>(data);
				bool ok = data2.contained == 123;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


			{
				string title = "contains<string>";
				var data = new contains<string>("Hello");
				var data2 = EasySerializeThenDeserialize<contains<string>>(data);
				bool ok = data2.contained == "Hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "MyGenericICollection";
				var data = new MyGenericICollection();
				data.Add(123);
				var data2 = EasySerializeThenDeserialize<MyGenericICollection>(data);
				bool ok = data2.Contains(123);
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


			{
				string title = "SomeItemsMoreProps";
				var data = new SomeItemsMoreProps();
				data.Add(123);
				data.s = "To be serialized";
				var data2 = EasySerializeThenDeserialize<SomeItemsMoreProps>(data);
				bool ok = data2[0] == 123 && data2.s == "To be serialized";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "MyNotGenericIList";
				var data = new MyNotGenericIList();
				data.Add(123);
				data.Add("hello");
				var data2 = EasySerializeThenDeserialize<MyNotGenericIList>(data);
				bool ok = (int)data2[0] == 123 && (string)data2[1] == "hello";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "MyDictionary";
				var data = new MyGenericDictionary();
				data.Add(123, 10.0);
				var data2 = EasySerializeThenDeserialize<MyGenericDictionary>(data);
				bool ok = data2[123] == 10.0;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "MyList";
				var data = new MyList();
				data.Add(123);
				var data2 = EasySerializeThenDeserialize<MyList>(data);
				bool ok = data2[0] == 123;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if (! SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)) && !NO_WPF_TESTS
			// Still (2013-08-29) have a problem with "System.Windows.Input.CursorType".
			{
				string title = "contains<TextBox>";
				var data = new contains<TextBox>(new TextBox());
				var data2 = EasySerializeThenDeserialize<contains<TextBox>>(data);
				bool ok = data2.contained is TextBox;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

#if (!SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)) && !NO_WPF_TESTS
			// Still (2013-08-29) have a problem with "System.Windows.Input.CursorType".
			{
				string title = "Two references to the same object.";
				var data = new TextBox[2];
				data[0] = new TextBox() { Text = "TextBox1" };
				data[1] = data[0]; // Same reference
				var data2 = EasySerializeThenDeserialize<TextBox[]>(data);
				data2[0].Text = "New text";
				bool sameReference = object.ReferenceEquals(data2[0], data2[1]);
				bool ok = sameReference && data2[1].Text == "New text";
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if ! SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)
			{
				string title = "MyICollectionMoreInsert";
				var data = new MyICollectionMoreInsert();
				data.AddForDebug(123);
				var data2 = EasySerializeThenDeserialize<MyICollectionMoreInsert>(data);
				var e = data2.GetEnumerator();
				e.MoveNext();
				bool ok = (int)e.Current == 123;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "MyICollectionMoreAdd";
				var data = new MyICollectionMoreAdd();
				data.AddForDebug(123);
				var data2 = EasySerializeThenDeserialize<MyICollectionMoreAdd>(data);
				var e = data2.GetEnumerator();
				e.MoveNext();
				bool ok = (int)e.Current == 123;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if ! SILVERLIGHT // Hashtable does not exist in Silverlight.
			{
				string title = "Hashtable";
				var data = new Hashtable();
				data.Add("hello", 123);
				var data2 = EasySerializeThenDeserialize<Hashtable>(data);
				bool ok = (int)data2["hello"] == 123;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{	// Since a pure ICollection has no Add nor Insert methods, we can not deserialize it.
				string title = "MyICollection";
				var data = new MyICollection();
				data.AddForDebug(123);
				try
				{
					var data2 = EasySerializeThenDeserialize<MyICollection>(data);
					var e = data2.GetEnumerator();
					e.MoveNext();
					bool ok = (int)e.Current == 123;
					ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				}
				catch { } // An exception is normal in that particular case.
				if (!AutomaticTests)
					Debugger.Break();
			}

#if ! SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)
			// Still (2013-08-29) have a problem with "System.Windows.Input.CursorType".
			{
				string title = "contains null";
				var data = new contains<TextBox>();
				var data2 = EasySerializeThenDeserialize<contains<TextBox>>(data);
				bool ok = data2.contained == null;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{
				string title = "null";
				TextBox data = null;
				var data2 = EasySerializeThenDeserialize<TextBox>(data);
				bool ok = data2 == null;
				ResultsLog.Add(title, ok);
				if (!AutomaticTests)
					Debugger.Break();
			}

			{
				string title = "SomeItemsInAIEnumerable";
				var data = new SomeItemsInAIEnumerable();
				try
				{
					var data2 = EasySerializeThenDeserialize<SomeItemsInAIEnumerable>(data);
					bool ok = data2 is SomeItemsInAIEnumerable;
					ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				}
				catch { } // An exception is normal in that particular case.
				if (!AutomaticTests)
					Debugger.Break();
			}

#if (!SILVERLIGHT || (SILVERLIGHT && ! LIMITED_RIGHTS_SILVERLIGHT)) && !NO_WPF_TESTS
			{ // 1) StackPanel inherits Panel which contains an essential private field: _uiElementCollection.
				// 2) There is a cycle because TextBox's parent is the reference of its container.

				string title = "contains<StackPanel>";
				var data = new contains<StackPanel>(new StackPanel());
				data.contained.Children.Add(new TextBox());
				var data2 = EasySerializeThenDeserialize<contains<StackPanel>>(data);
				bool ok = data2.contained is StackPanel;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !NO_WPF_TESTS

#if !LIMITED_RIGHTS_SILVERLIGHT
			{
				string title = "contains<FontFamily>";
				var data = new contains<FontFamily>(window.FontFamily); // [ValueSerializer]
#if SILVERLIGHT
				var data2 = EasySerializeThenDeserialize<contains<FontFamily>>(data);
#else
				var data2 = EasySerializeThenDeserializeWPF<contains<FontFamily>>(data);
#endif
				bool ok = data2.contained.ToString() == data.contained.ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

			{ // System.Windows.Media.ImageSource -> Contiendra un System.Windows.Media.Imaging.BitmapFrameDecode.
				string title = "contains<System.Windows.Media.ImageSource>";
#if SILVERLIGHT
				var data = new contains<System.Windows.Media.ImageSource>(window.MyIcon.Source);
				var data2 = EasySerializeThenDeserialize<contains<System.Windows.Media.ImageSource>>(data);
#else
				var data = new contains<System.Windows.Media.ImageSource>(window.Icon);
				var data2 = EasySerializeThenDeserializeWPF<contains<System.Windows.Media.ImageSource>>(data);
#endif
				bool ok = data2.contained.ToString() == data.contained.ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}


#if !LIMITED_RIGHTS_SILVERLIGHT
			{
				string title = "FontFamily";
				var data = window.FontFamily; // [ValueSerializer]
#if SILVERLIGHT
				var data2 = EasySerializeThenDeserialize<FontFamily>(data);
#else
				var data2 = EasySerializeThenDeserializeWPF<FontFamily>(data);
#endif
				bool ok = data2.ToString() == data.ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif
#endif // !NO_WPF_TESTS



#if !NO_WPF_TESTS
			{
				string title = "System.Uri (TypeConverter & Serializable).";
#if SILVERLIGHT
				var data = (window.MyIcon.Source as System.Windows.Media.Imaging.BitmapImage).UriSource;
#else
				var data = new System.Uri(window.Icon.ToString()); // [Serializable]
#endif
				var data2 = EasySerializeThenDeserialize<System.Uri>(data);
				bool ok = data2.ToString() == data.ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !LIMITED_RIGHTS_SILVERLIGHT
			{
				string title = "Cursor (TypeConverter)";
				var data = window.Cursor; // [TypeConverter]
				var data2 = EasySerializeThenDeserialize<Cursor>(data);
				bool ok = data2.ToString() == data.ToString();
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif





#if !SILVERLIGHT
			{
				string title = "TextDecorationCollection";
				var data = new TextDecorationCollection();
				data.Add(new TextDecoration());
				var data2 = EasySerializeThenDeserialize<TextDecorationCollection>(data);
				bool ok = data2[0] is TextDecoration;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !SILVERLIGHT
			{
				string title = "'Type' fits in CLRTypeContainer.";
				var data = typeof(string);
				var data2 = EasySerializeThenDeserialize<Type>(data);
				bool ok = data2 == data;
				ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
				if (!AutomaticTests)
					Debugger.Break();
			}
#endif

#if !NO_WPF_TESTS
			{
#if !SILVERLIGHT
				if (window.Title != "fen2")
#endif
				{
					string title = "fen2";
					var data = new
#if SILVERLIGHT
 MainPage() { Cursor = Cursors.Hand, FontFamily = window.FontFamily, FontSize = 16 };

#else
 Window() { Title = "Fen 2", Cursor = Cursors.Hand, Icon = window.Icon, FontFamily = window.FontFamily, FontSize = 16 };
#endif


					var sp = new StackPanel();
					sp.Orientation = Orientation.Vertical;
					sp.Background = new SolidColorBrush((new Color() { R = 20, G = 120, B = 220 }));

					var tb = new TextBlock();
					//var tb = new TextBox();
					tb.Text = "Hello fen 2 !";
					tb.Width = 300;
					tb.Height = 60;
#if !SILVERLIGHT
					tb.Background =
												Brushes.Firebrick;
#endif
					sp.Children.Add(tb);

					Button b = new Button();
					b.Content = "Click me !";
					b.Width = 300;
					b.Height = 60;
					sp.Children.Add(b);

					data.Content = sp;

#if !SILVERLIGHT
					var data2 = EasySerializeThenDeserializeWPF<Window>(data);
					data2.Title = "fen2";
					data2.ShowDialog();
#else
					byte[] bytes = UniversalSerializer.Serialize(data);
					var data2 = UniversalSerializer.Deserialize<MainPage>(bytes);
#endif

					bool ok = (data2.Content as StackPanel).Children[0] is TextBlock;
					ResultsLog.Add(title + " {" + data.GetType().GetName() + "}", ok);
					if (!AutomaticTests)
						Debugger.Break();
					data.Close();
					data2.Close();
				}
			}
#endif
#endif // !NO_WPF_TESTS

			#endregion simple_tests

			// - - - -

			{
				List<string> Failed = new List<string>();
				bool AllOK =
					ResultsLog.All(p =>
						{
							if (!p.Value)
								Failed.Add(p.Key);
							return p.Value;
						});
				Debugger.Break();
			}

		}

		static T EasySerializeThenDeserialize<T>(T source)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Parameters parameters = new Parameters() { Stream = ms, SerializerFormatter = TestSerFormatter };
				UniversalSerializer ser = new UniversalSerializer(parameters);
				ser.Serialize(source);
#if DEBUG_WriteSerializedFile
				if (Directory.Exists(@"r:\"))
				{
					using (FileStream fs = new FileStream(@"r:\serialized.data", FileMode.Create, FileAccess.ReadWrite))
					{
						ms.Flush();
						ms.Position = 0;
						ms.CopyTo(fs);
					}
				}
#endif
				return ser.Deserialize<T>();
			}
		}

		static DescriptorResult EasySerializeThenDeserializeWithDescriptor(
			DataDescriptor dataDescriptor, int ItemCount, Dictionary<string, bool> Results)
		{
			DescriptorResult r = new DescriptorResult();

			using (MemoryStream ms = new MemoryStream())
			{
				r.original = dataDescriptor.BuildASampleArray(ItemCount);
				Parameters parameters = new Parameters() { Stream = ms, SerializerFormatter = TestSerFormatter };
				using (UniversalSerializer ser = new UniversalSerializer(parameters))
				{
					ser.Serialize(r.original);

#if DEBUG_WriteSerializedFile
					if (Directory.Exists(@"r:\"))
					{
						string n;
						switch (TestSerFormatter)
						{
							case SerializerFormatters.BinarySerializationFormatter:
								n = @"r:\serialized.data";
								break;
#if !PORTABLE
							case SerializerFormatters.JSONSerializationFormatter:
								n = @"r:\serialized.json";
								break;
#endif
							case SerializerFormatters.XmlSerializationFormatter:
								n = @"r:\serialized.xml";
								break;
						}

						using (FileStream fs = new FileStream(n, FileMode.Create, FileAccess.ReadWrite))
						{
							ms.Flush();
							ms.Position = 0;
							ms.CopyTo(fs);
						}
					}
#endif

					r.deserialized = (Array)ser.Deserialize();
				}
				bool ok = dataDescriptor.Check10Items(r.original, r.deserialized);
				Results.Add(dataDescriptor.Description, ok);


				return r;
			}
		}

		struct DescriptorResult
		{
			public Array original;
			public Array deserialized;
		}

#if !SILVERLIGHT && !NO_WPF_TESTS
		static T EasySerializeThenDeserializeWPF<T>(T source)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Parameters parameters = new Parameters() { Stream = ms, SerializerFormatter = TestSerFormatter };
				var ww = Application.Current.Windows;
				using (UniversalSerializerWPF ser = new UniversalSerializerWPF(parameters))
				{
					ww = Application.Current.Windows;
					ser.Serialize(source);
					ww = Application.Current.Windows;
#if DEBUG_WriteSerializedFile
				if (Directory.Exists(@"r:\"))
				{
					using (FileStream fs = new FileStream(@"r:\serialized.data", FileMode.Create, FileAccess.ReadWrite))
					{
						ms.Flush();
						ms.Position = 0;
						ms.CopyTo(fs);
					}
				}
#endif
					return ser.Deserialize<T>();
				}
			}
		}
#endif

		// ===============================================================================
		// ===============================================================================

		static void CustomContainerTest()
		{
			{
				/* This example needs a custom ITypeContainer.
				Normally, this class can not be serialized (see details in its source).
				But thanks to this container, we can serialize the class as a small data (an integer).
				 */

				var data = new MyStrangeClassNeedsACustomContainer(123);

				using (MemoryStream ms = new MemoryStream())
				{
					var p = new Parameters() { Stream = ms, SerializerFormatter = TestSerFormatter };
					UniversalSerializer ser = new UniversalSerializer(p);

					ser.Serialize(data);
					var data2 = ser.Deserialize<MyStrangeClassNeedsACustomContainer>();

					bool ok = data2.ATextBox.Text == "123";
					Debugger.Break();
				}

			}
		}

		public class CustomContainerTestModifiers : CustomModifiers
		{
			public CustomContainerTestModifiers()
				: base(Containers: new ITypeContainer[] {
					new ContainerForMyStrangeClass()
					})
			{
			}
		}

		// ===============================================================================
		// ===============================================================================

		static void ExternalCustomContainerTest()
		{
#if !NETFX_CORE // Windows Store 8.
			{
				/* This example needs an external custom ITypeContainer.
				Normally, this class can not be serialized (see details in its source).
				But thanks to this container, we can serialize the class as a small data (an integer).
				 */

				var data = new ThisClassNeedsAnExternalCustomContainer(123);

				using (MemoryStream ms = new MemoryStream())
				{
					var p = new Parameters()
					{
						Stream = ms,
						SerializerFormatter = TestSerFormatter,
						ModifiersAssemblies = new ModifiersAssembly[1] { ModifiersAssembly.GetModifiersAssembly( 
#if SILVERLIGHT
							"ExternalModifiers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
#else
							"ExternalModifiers"
#endif
							) }
					};
					UniversalSerializer ser = new UniversalSerializer(p);

					ser.Serialize(data);
					var data2 = ser.Deserialize<ThisClassNeedsAnExternalCustomContainer>();

					bool ok = data2.IntegerText == "123";
					Debugger.Break();
				}

			}
#endif
		}

		// ===============================================================================
		// ===============================================================================

		private enum PrivateEnum { FirstAndDefault, Something }

		public enum PublicEnum { FirstAndDefault, Something }

		public struct Box<T>
		{
			public T value;
		}

		public struct MyColor
		{
			public int R, G, B;
		}

		public class Integers : IEquatable<Integers>
		{
			public byte Byte = 0xfe;
			public sbyte SByte = 0x12;
			public ushort UShort = 0xfedc;
			public short Short = 0x1234;
			public uint UInt = 0xfedcba98;
			public int Int = 0x12345678;
			public ulong ULong = 0xfedcba9876543210;
			public long Long = 0x0123456789abcdef;

			public bool Equals(Integers other)
			{
				if (other == null)
					return false;
				return
					this.Byte == other.Byte
					&& this.SByte == other.SByte
					&& this.UShort == other.UShort
					&& this.Short == other.Short
					&& this.UInt == other.UInt
					&& this.Int == other.Int
					&& this.ULong == other.ULong
					&& this.Long == other.Long;
			}

			public override bool Equals(object obj)
			{
				return this.Equals((Integers)obj);
			}

			public override int GetHashCode() // only for the compiler.
			{
				return base.GetHashCode();
			}
		}

		/// <summary>
		/// This class only contains an integer.
		/// </summary>
		public class VerySimpleClass
		{
			public int a;
		}

		/// <summary>
		/// A private class, with a private constructor.
		/// </summary>
		private class PrivateClass
		{
		}

		public class ParametricConstructedClassWithPublicField
		{
			public readonly int[] IntArray;

			public ParametricConstructedClassWithPublicField(int[] intArray)
			{
				this.IntArray = intArray;
			}

			public override bool Equals(object obj)
			{
				ParametricConstructedClassWithPublicField o = obj as ParametricConstructedClassWithPublicField;
				if (o == null || this.IntArray.Length != o.IntArray.Length)
					return false;
				for (int i = 0; i < this.IntArray.Length; i++)
					if (this.IntArray[i] != o.IntArray[i])
						return false;
				return true;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		/// <summary>
		/// . No default (no-param) constructor.
		/// . The only constructor has a parameter with no corresponding field.
		/// . ATextBox has a private 'set' and is different type from constructor's parameter.
		/// </summary>
		public class MyStrangeClassNeedsACustomContainer
		{
			/// <summary>
			/// It is built from the constructor's parameter.
			/// Since its 'set' method is not public, it will not be serialized directly.
			/// </summary>
			public FakeTextBox ATextBox { get; private set; }

			public MyStrangeClassNeedsACustomContainer(int NumberAsTitle)
			{
				this.ATextBox = new FakeTextBox() { Text = NumberAsTitle.ToString() };
			}
		}

		/// <summary>
		/// A minimalist fake TextBox control.
		/// </summary>
		public class FakeTextBox
		{
			public string Text { get; set; }
		}

		class ContainerForMyStrangeClass : ITypeContainer
		{
			#region Here you add data to be serialized in place of the class instance

			public int AnInteger; // We store the smallest, sufficient and necessary data.

			#endregion Here you add data to be serialized in place of the class instance


			public ITypeContainer CreateNewContainer(object ContainedObject)
			{
				MyStrangeClassNeedsACustomContainer sourceInstance = ContainedObject as MyStrangeClassNeedsACustomContainer;
				return new ContainerForMyStrangeClass() { AnInteger = int.Parse(sourceInstance.ATextBox.Text) };
			}

			public object Deserialize()
			{
				return new MyStrangeClassNeedsACustomContainer(this.AnInteger);
			}

			public bool IsValidType(Type type)
			{
				return Tools.TypeIs(type, typeof(MyStrangeClassNeedsACustomContainer));
			}

			public bool ApplyEvenIfThereIsAValidConstructor
			{
				get { return false; }
			}

			public bool ApplyToStructures
			{
				get { return false; }
			}
		}


		// ===============================================================================
		// ===============================================================================

		static void CustomFiltersTest()
		{
			{
				/* This example needs custom filters.
				Normally, this class can be serialized but with wrong fields.
				Thanks to these filters, we can serialize the class appropriately.
				 */

				using (MemoryStream ms = new MemoryStream())
				{
					var p = new Parameters() { Stream = ms, SerializerFormatter = TestSerFormatter };
					var ser = new UniversalSerializer(p);

					var data = new ThisClassNeedsFilters(123);
					ser.Serialize(data);
					var data2 = ser.Deserialize<ThisClassNeedsFilters>();

					bool ok = data2.Value == "123" && data2.Useless == null;
					Debugger.Break();
				}
			}
		}

		public class CustomFiltersTestModifier : CustomModifiers
		{
			public CustomFiltersTestModifier()
				: base(FilterSets: new FilterSet[] {
 					new FilterSet() { 
						AdditionalPrivateFieldsAdder=MyAdditionalPrivateFieldsAdder, 
						TypeSerializationValidator=MyTypeSerializationValidator } })
			{
			}
		}

		// ===============================================================================

		static void ShareTest()
		{
			/* Here we serialize a serie of data, sharing the same serializer and deserializer.
			 */

#if false // One sharing test is enough.
			using (var stream = new MemoryStream())
			{
				var p = new Parameters() { Stream = stream, SerializerFormatter = TestSerFormatter };
				var serializer = new UniversalSerializer(p);
				byte data = 1;
				var pars = new Parameters();
				pars.Stream = stream;
				bool ok = true;

				for (int i = 0; i < 10; i++)
				{
					long streamPosition = stream.Position;
					serializer.Serialize(data);
					stream.Position = streamPosition; // we need to deserialize from the right position.
					var data2 = serializer.Deserialize<byte>();
					ok &= data2 == data++;
				}

				Debugger.Break(); // check 'ok'.
			}

			using (var stream = new MemoryStream())
			{
				var p = new Parameters() { Stream = stream, SerializerFormatter = TestSerFormatter };
				var serializer = new UniversalSerializer(p);
				MyByteColorArray.MyByteColor data;
				var pars = new Parameters();
				pars.Stream = stream;
				bool ok = true;

				for (int i = 0; i < 10; i++)
				{
					data = new MyByteColorArray.MyByteColor() { R = (byte)i };
					long streamPosition = stream.Position;
					serializer.Serialize(data);
					stream.Position = streamPosition; // we need to deserialize from the right position.
					var data2 = serializer.Deserialize<MyByteColorArray.MyByteColor>();
					ok &= data2 == data;

				}

				Debugger.Break(); // check 'ok'.
			}
#endif

			using (var stream = new MemoryStream())
			{
				var p = new Parameters() { Stream = stream, SerializerFormatter = TestSerFormatter };
				var serializer = new UniversalSerializer(p);
				var data = new Test_UniversalSerializer.Tests.color() { R = 100, G = 200, B = 250, Transparency = new TransparentMask() { mask = 50 } };
				var pars = new Parameters();
				pars.Stream = stream;
				bool ok = true;

				for (int i = 0; i < 10; i++)
				{
					data.R = i + 100;
					long streamPosition = stream.Position;
					serializer.Serialize(data);
					stream.Position = streamPosition; // we need to deserialize from the right position.
					var data2 = serializer.Deserialize<color>();
					ok &= data2.R == data.R;
				}

				Debugger.Break(); // check 'ok'.
			}
		}

		// ===============================================================================

		public class ThisClassNeedsFilters
		{
			public ShouldNotBeSerialized Useless;
			private int Integer;
			public string Value { get { return this.Integer.ToString(); } }

			public ThisClassNeedsFilters()
			{
			}

			public ThisClassNeedsFilters(int a)
			{
				this.Integer = a;
				this.Useless = new ShouldNotBeSerialized();
			}
		}

		public class ShouldNotBeSerialized
		{
		}

		/// <summary>
		/// Tells the serializer to add some certain private fields to store the type.
		/// </summary>
		static FieldInfo[] MyAdditionalPrivateFieldsAdder(Type t)
		{
			if (Tools.TypeIs(t, typeof(ThisClassNeedsFilters)))
				return new FieldInfo[] { Tools.FieldInfoFromName(t, "Integer") };
			return null;
		}

		/// <summary>
		/// Returns 'false' if this type should not be serialized at all.
		/// That will let the default value created by the constructor of its container class/structure.
		/// </summary>
		static bool MyTypeSerializationValidator(Type t)
		{
			return !Tools.TypeIs(t, typeof(ShouldNotBeSerialized));
		}

		// ===============================================================================
		// ===============================================================================

		public class GenericClassFromGenericIntListWithOtherType<T> : List<int>
		{
			public T Value;
		}

		public class MyGenericICollectionWithoutNoParamConstructor : MyGenericICollection
		{
			private int _field;

			public MyGenericICollectionWithoutNoParamConstructor(int field)
			{
				this._field = field;
			}
		}

		public class MyDictPresentedAsAICollection : ICollection
		{
			Dictionary<object, int> internalDict = new Dictionary<object, int>();

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return this.internalDict.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerator GetEnumerator()
			{
				return this.internalDict.GetEnumerator();
			}
		}

		public class MyICollectionMoreAdd : ICollection
		{
			List<int> internalList = new List<int>();

			/// <summary>
			/// Not part of ICollection, but very common therefore usable by the serializer.
			/// </summary>
			void Add(int a)
			{
				this.internalList.Add(a);
			}

			public void AddForDebug(int a)
			{
				this.internalList.Add(a);
			}

			public void CopyTo(Array array, int index)
			{
				this.internalList.CopyTo(array as int[], index);
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}


		public class MyICollectionMoreInsert : ICollection
		{
			List<int> internalList = new List<int>();

			/// <summary>
			/// Not part of ICollection, but very common therefore usable by the serializer.
			/// </summary>
			void Insert(int index, int value)
			{
				this.internalList.Insert(index, value);
			}

			public void AddForDebug(int a)
			{
				this.internalList.Add(a);
			}

			public void CopyTo(Array array, int index)
			{
				this.internalList.CopyTo(array as int[], index);
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}



		public class MyICollection : ICollection
		{
			List<int> internalList = new List<int>();

			public void AddForDebug(int a)
			{
				this.internalList.Add(a);
			}

			public void CopyTo(Array array, int index)
			{
				this.internalList.CopyTo(array as int[], index);
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}

		public class MyGenericICollection : ICollection<int>
		{
			List<int> internalList = new List<int>();

			public override string ToString()
			{
				return this.internalList.ToString();
			}

			public void Add(int item)
			{
				this.internalList.Add(item);
			}

			public void Clear()
			{
				this.internalList.Clear();
			}

			public bool Contains(int item)
			{
				return this.internalList.Contains(item);
			}

			public void CopyTo(int[] array, int arrayIndex)
			{
				this.internalList.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public bool Remove(int item)
			{
				return this.internalList.Remove(item);
			}

			public IEnumerator<int> GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}


		public class MyGenericDictionary : Dictionary<int, double> { }

		public class MyList : List<int> { }

		public class SomeItemsMoreProps : List<int>
		{
			public string s = "A prop";

			public override string ToString()
			{
				return this.s + " ; " + base.ToString();
			}
		}

		/// <summary>
		/// A generic collection that is not a IList (it is not indexed).
		/// Useful for other test classes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class GenericICollection<T> : ICollection<T>
		{
			List<T> internalList = new List<T>();

			public override string ToString()
			{
				return this.internalList.ToString();
			}

			public void Add(T item)
			{
				this.internalList.Add(item);
			}

			public void Clear()
			{
				this.internalList.Clear();
			}

			public bool Contains(T item)
			{
				return this.internalList.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				this.internalList.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public bool Remove(T item)
			{
				return this.internalList.Remove(item);
			}

			public IEnumerator<T> GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}



		public class SomeItemsInAIEnumerable : System.Collections.IEnumerable
		{
			List<object> internalList = new List<object>();
			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}

		public class contains<T>
		{
			public T contained;
			public contains() { }
			public contains(T data)
			{
				this.contained = data;
			}
		}

		public class SomeItemsInAIList : System.Collections.IList
		{
			List<object> internalList = new List<object>();
			public int Add(object value)
			{
				this.internalList.Add(value);
				return this.internalList.Count - 1;
			}

			public void Clear()
			{
				this.internalList.Clear();
			}

			public bool Contains(object value)
			{
				return this.internalList.Contains(value);
			}

			public int IndexOf(object value)
			{
				return this.internalList.IndexOf(value);
			}

			public void Insert(int index, object value)
			{
				this.internalList.Insert(index, value);
			}

			public bool IsFixedSize
			{
				get { return false; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public void Remove(object value)
			{
				this.internalList.Remove(value);
			}

			public void RemoveAt(int index)
			{
				this.internalList.RemoveAt(index);
			}

			public object this[int index]
			{
				get
				{
					return this.internalList[index];
				}
				set
				{
					this.internalList[index] = value;
				}
			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsSynchronized
			{
				get { return true; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}

		static byte[] DataContractSerializer(object obj)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				var formatter = new DataContractSerializer(obj.GetType());
				formatter.WriteObject(stream, obj);
				return stream.ToArray();
			}
		}

#if !SILVERLIGHT && !NETFX_CORE
		static byte[] BinarySerializer(object obj)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		static byte[] SoapSerializer(object obj)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				var formatter = new SoapFormatter();
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		static string JSSerializer(object obj)
		{
			var jser = new JavaScriptSerializer();
			var jsonText = jser.Serialize(obj);
			return jsonText;
		}

#endif

		static byte[] XMLSerializer(object obj)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				XmlSerializer xml = new XmlSerializer(obj.GetType());
				xml.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

#if false
		static void AnalyseConstructors(object obj)
		{
			Type t = obj.GetType();
			bool cherchePrivés = !t.IsPublic;
			BindingFlags bf =
				cherchePrivés ?
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
				: BindingFlags.Public | BindingFlags.Instance;
			var cs = t.GetConstructors(bf);
			var ms = t.GetMethods();
		}

		static void TestTypeConverter(object obj)
		{
			Type t = obj.GetType();
			var tca = t.GetCustomAttributes(typeof(System.ComponentModel.TypeConverterAttribute), true);
			TypeConverter tc = System.Activator.CreateInstance(Type.GetType((tca[0] as System.ComponentModel.TypeConverterAttribute).ConverterTypeName)) as TypeConverter;
			object transcoded;
			object reverted;
			if (tc.CanConvertTo(typeof(string)))
			{
				transcoded = tc.ConvertTo(obj, typeof(string));
				if (tc.CanConvertFrom(typeof(string)))
					reverted = tc.ConvertFrom(transcoded);
			}
		}
#endif

		public class MyNotGenericIList : System.Collections.IList
		{
			List<object> internalList = new List<object>();

			public int Add(object value)
			{
				this.internalList.Add(value);
				return this.internalList.Count - 1;
			}

			public void Clear()
			{
				this.internalList.Clear();
			}

			public bool Contains(object value)
			{
				return this.internalList.Contains(value);
			}

			public int IndexOf(object value)
			{
				return this.internalList.IndexOf(value);
			}

			public void Insert(int index, object value)
			{
				this.internalList.Insert(index, value);
			}

			public bool IsFixedSize
			{
				get { return false; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public void Remove(object value)
			{
				this.internalList.Remove(value);
			}

			public void RemoveAt(int index)
			{
				this.internalList.RemoveAt(index);
			}

			public object this[int index]
			{
				get
				{
					return this.internalList[index];
				}
				set
				{
					this.internalList[index] = value;
				}
			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return this.internalList.Count; }
			}

			public bool IsSynchronized
			{
				get { return true; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.internalList.GetEnumerator();
			}
		}

		public class MyGenericIDictionaryMoreProps : IDictionary<int, string> // Does not inherit IDictionary.
		{
			public string MyField = "default text";
			Dictionary<int, string> internalDictionary = new Dictionary<int, string>();


			public void Add(int key, string value)
			{
				this.internalDictionary.Add(key, value);
			}

			public bool ContainsKey(int key)
			{
				return this.internalDictionary.ContainsKey(key);
			}

			public ICollection<int> Keys
			{
				get { return this.internalDictionary.Keys; }
			}

			public bool Remove(int key)
			{
				return this.internalDictionary.Remove(key);
			}

			public bool TryGetValue(int key, out string value)
			{
				return this.internalDictionary.TryGetValue(key, out value);
			}

			public ICollection<string> Values
			{
				get { return this.internalDictionary.Values; }
			}

			public string this[int key]
			{
				get
				{
					return this.internalDictionary[key];
				}
				set
				{
					this.internalDictionary[key] = value;
				}
			}

			public void Add(KeyValuePair<int, string> item)
			{
				this.internalDictionary.Add(item.Key, item.Value);
			}

			public void Clear()
			{
				this.internalDictionary.Clear();
			}

			public bool Contains(KeyValuePair<int, string> item)
			{
				return this.internalDictionary.Contains(item);
			}

			public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return this.internalDictionary.Count; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public bool Remove(KeyValuePair<int, string> item)
			{
				return this.internalDictionary.Remove(item.Key);
			}

			public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
			{
				return this.internalDictionary.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.internalDictionary.GetEnumerator();
			}
		}

		// ######################################################################

		public class Pixel
		{
			public int X, Y;
			public color Color;
		}
		public struct color
		{
			public int R, G, B;
			public Object Something; // to test objects.
			public Pixel ParentPixel; // to test cycles.
			public TransparentMask Transparency; // to test instance in a struct in an instance.
		}
		public class TransparentMask
		{
			public int mask;
		}

		// ######################################################################

		public class PrimitiveTypes
		{
			public bool Boolean = true;
			public DateTime Date = DateTime.MaxValue;
			public sbyte Integer8 = -0x12;
			public byte UnsignedInteger8 = 0xF2;
			public short Integer16 = -0x1234;
			public ushort UnsignedInteger16 = 0xF234;
			public int Integer32 = -0x12345678;
			public uint UnsignedInteger32 = 0xF2345678U;
			public long Integer64 = -0x123456789abcdef0;
			public ulong UnsignedInteger64 = 0xF23456789abcdef0UL;
			public Single SingleFloat = 1.23e12F;
			public double DoubleFloat = 1.23e45;
			public Decimal DecimalNumber = 79228162514264337593543950335M;
			public char Character = '開';
			public string CharacterString = "開à\xfffdóT";

			public override bool Equals(object obj)
			{
				PrimitiveTypes b = obj as PrimitiveTypes;
				if (b == null)
					throw new ArgumentException();
				return
					this.Boolean == b.Boolean
					&& this.Character == b.Character
					&& this.CharacterString == b.CharacterString
					&& this.Date == b.Date
					&& this.DecimalNumber == b.DecimalNumber
					&& this.DoubleFloat == b.DoubleFloat
					&& this.Integer16 == b.Integer16
					&& this.Integer32 == b.Integer32
					&& this.Integer64 == b.Integer64
					&& this.Integer8 == b.Integer8
					&& this.SingleFloat == b.SingleFloat
					&& this.UnsignedInteger16 == b.UnsignedInteger16
					&& this.UnsignedInteger32 == b.UnsignedInteger32
					&& this.UnsignedInteger64 == b.UnsignedInteger64
					&& this.UnsignedInteger8 == b.UnsignedInteger8;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		// ######################################################################

		public class MyObjectIDictionary : IDictionary
		{
			Dictionary<object, object> internalDict = new Dictionary<object, object>();

			public void Add(object key, object value)
			{
				this.internalDict.Add(key, value);
			}

			public void Clear()
			{
				this.internalDict.Clear();
			}

			public bool Contains(object key)
			{
				throw new NotImplementedException();
			}

			public IDictionaryEnumerator GetEnumerator()
			{
				return this.internalDict.GetEnumerator();
			}

			public bool IsFixedSize
			{
				get { return false; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}

			public ICollection Keys
			{
				get { return this.internalDict.Keys; }
			}

			public void Remove(object key)
			{
				this.internalDict.Remove(key);
			}

			public ICollection Values
			{
				get { return this.internalDict.Values; }
			}

			public object this[object key]
			{
				get
				{
					return this.internalDict[key];
				}
				set
				{
					this.internalDict[key] = value;
				}
			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return this.internalDict.Count; }
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.internalDict.GetEnumerator();
			}
		}

#if !NO_WPF_TESTS
		public class DerivedWindow :
#if SILVERLIGHT || NETFX_CORE
 MainPage
#else
 Window
#endif
		{
			public DerivedWindow()
			{
			}
		}
#endif

		// ######################################################################
		// ######################################################################

		public class CircularType
		{
			public int Id;

			public CircularType SubItem;
		}


		// #####################################################################
		// ######################################################################

		public class CircularTypeWithGenericList
		{
			public int Id { get; set; }
			public string Name { get; set; }

			public List<CircularTypeWithGenericList> others = new List<CircularTypeWithGenericList>();
		}

		// #####################################################################
		// ######################################################################

		/// <summary>
		/// This class causes problems because:
		/// 1. It has no exploitable constructor (no default constructor, no parametric constructor with adequate type and name of parameters).
		/// 2. The BCL's binary formatter serializer can not serialize it either because of a sub class 'NotMarkedAsSerializable'.
		/// </summary>
#if !NETFX_CORE
		[System.SerializableAttribute]
#endif
		public class NotCompatibleWithSerializableAttribute
		{
			public NotCompatibleWithSerializableAttribute(NotMarkedAsSerializable NotFieldName)
			{
				this.SubData = NotFieldName;
			}

			public NotMarkedAsSerializable SubData;

			public class NotMarkedAsSerializable
			{
			}
		}

		// #####################################################################
		// ######################################################################

		public class ForceSerializeOnPrivateFieldAndProperty
		{
			[ForceSerialize]
			private double[] ForcedPrivateProperty { get; set; }
			[ForceSerialize]
			private double[] ForcedPrivateField;

			public ForceSerializeOnPrivateFieldAndProperty() { }

			public ForceSerializeOnPrivateFieldAndProperty(params double[] originalCoords)
			{
				this.ForcedPrivateProperty = new double[originalCoords.Length];
				originalCoords.CopyTo(this.ForcedPrivateProperty, 0);

				this.ForcedPrivateField = new double[originalCoords.Length];
				originalCoords.CopyTo(this.ForcedPrivateField, 0);
			}

			public double this[int index]
			{
				get { return this.ForcedPrivateProperty[index] + this.ForcedPrivateField[index]; }
			}
		}

		// #####################################################################
		// ######################################################################

		public class CircularDictionaryInAField
		{
			public Dictionary<int, CircularDictionaryInAField> Dict;
		}

		// #####################################################################
		// ######################################################################

		public class CircularListInAField
		{
			public List<CircularListInAField> List;
		}

		// #####################################################################
		// ######################################################################

		#region Advanced circular tests

		public class CircularTests
		{
			public CircularInstancesInACollectionAndParametricConstructor c0;
			public CircularInstancesInAListAndParametricConstructor c1;
			public CircularInstanceInAFieldAndParametricConstructor c3;
			public CircularInstanceInAPropertyAndParametricConstructor c4;
			public CircularInstancesInADictionaryAndParametricConstructor c2;

			public CircularTests()
			{
#if ready
				this.c0 = new CircularInstancesInACollectionAndParametricConstructor("c0");
				this.c0.Instances.Add(null);
				this.c0.Instances.Add(c0);
				this.c0.Instances.Add(null);
#endif

				this.c1 = new CircularInstancesInAListAndParametricConstructor("c1");
				this.c1.Instances.Add(null);
				this.c1.Instances.Add(c1);
				this.c1.Instances.Add(null);

				this.c3 = new CircularInstanceInAFieldAndParametricConstructor("c3");
				this.c3.Instance = c3;

				this.c4 = new CircularInstanceInAPropertyAndParametricConstructor("c4");
				this.c4.Instance = c4;

				this.c2 = new CircularInstancesInADictionaryAndParametricConstructor("c2");
				this.c2.InstancesInKey.Add(c2, 21);
#if ready
				this.c2.InstancesInKeyAndInValue.Add(c2, c2);
#endif
				this.c2.InstancesInValue.Add(22, c2);
			}

			public bool TestIntegrity()
			{
				return
#if ready
					this.c0 != null
					&& this.c0.Instances != null
					&& this.c0.Instances.Count == 3
					&& this.c0.Instances.ToArray()[1] == c0
					&&
#endif
 this.c1 != null
					&& this.c1.Instances != null
					&& this.c1.Instances.Count == 3
					&& this.c1.Instances[1] == c1
					&& this.c2 != null
					&& this.c2.InstancesInKey != null
					&& this.c2.InstancesInKey.ContainsKey(this.c2)
					&& this.c2.InstancesInKey[this.c2] == 21
					&& this.c2.InstancesInValue != null
					&& this.c2.InstancesInValue.ContainsKey(22)
					&& this.c2.InstancesInValue[22] == this.c2
#if ready
					&& this.c2.InstancesInKeyAndInValue != null
					&& this.c2.InstancesInKeyAndInValue.ContainsKey(this.c2)
					&& this.c2.InstancesInKeyAndInValue[this.c2] == this.c2
#endif
 && this.c3 != null
					&& this.c3.Instance == this.c3
					&& this.c4 != null
					&& this.c4.Instance == this.c4;
			}
		}

		/// <summary>
		/// . No default constructor, only a parametric constructor.
		/// . Possible cirular references in a list.
		/// </summary>
		public class CircularInstancesInAListAndParametricConstructor
		{
			public string Name;
			public List<CircularInstancesInAListAndParametricConstructor> Instances
				= new List<CircularInstancesInAListAndParametricConstructor>();

			public CircularInstancesInAListAndParametricConstructor(string Name)
			{
				this.Name = Name;
			}
			public override string ToString()
			{
				return this.Name;
			}
		}

		public class CircularInstancesInACollectionAndParametricConstructor
		{
			public string Name;
			public GenericICollection<CircularInstancesInACollectionAndParametricConstructor> Instances
				= new GenericICollection<CircularInstancesInACollectionAndParametricConstructor>();

			public CircularInstancesInACollectionAndParametricConstructor(string Name)
			{
				this.Name = Name;
			}
			public override string ToString()
			{
				return this.Name;
			}
		}

		public class CircularInstancesInADictionaryAndParametricConstructor
		{
			public string Name;
			public Dictionary<int, CircularInstancesInADictionaryAndParametricConstructor> InstancesInValue =
				new Dictionary<int, CircularInstancesInADictionaryAndParametricConstructor>();
			public Dictionary<CircularInstancesInADictionaryAndParametricConstructor, int> InstancesInKey =
				new Dictionary<CircularInstancesInADictionaryAndParametricConstructor, int>();
			public Dictionary<CircularInstancesInADictionaryAndParametricConstructor, CircularInstancesInADictionaryAndParametricConstructor> InstancesInKeyAndInValue =
				new Dictionary<CircularInstancesInADictionaryAndParametricConstructor, CircularInstancesInADictionaryAndParametricConstructor>();

			public CircularInstancesInADictionaryAndParametricConstructor(string Name)
			{
				this.Name = Name;
			}
			public override string ToString()
			{
				return this.Name;
			}
		}

		public class CircularInstanceInAFieldAndParametricConstructor
		{
			public string Name;
			public CircularInstanceInAFieldAndParametricConstructor Instance;

			public CircularInstanceInAFieldAndParametricConstructor(string Name)
			{
				this.Name = Name;
			}
			public override string ToString()
			{
				return this.Name;
			}
		}

		public class CircularInstanceInAPropertyAndParametricConstructor
		{
			public string Name;
			public CircularInstanceInAPropertyAndParametricConstructor Instance { get; set; }

			public CircularInstanceInAPropertyAndParametricConstructor(string Name)
			{
				this.Name = Name;
			}
			public override string ToString()
			{
				return this.Name;
			}
		}

		#endregion Advanced circular tests

		// #####################################################################
		// ######################################################################

		public abstract class ClassWithPrivateField
		{
			private int value;

			public int PrivateFieldGetterX2 { get { return this.value * 2; } }

			public ClassWithPrivateField(int Value)
			{
				this.value = Value;
			}
		}

		public class ClassInheritingAPrivateField : ClassWithPrivateField
		{

			public ClassInheritingAPrivateField(int Value)
				: base(Value)
			{ }
		}

		// ######################################################################
		// ######################################################################
		// ######################################################################
	}


}
