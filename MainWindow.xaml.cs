using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LinqReflection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ReferencedAssemblyNames { get; set; }

        public string Assemblies { get; set; }

        public string TemplateClasses { get; set; }

        public string TemplateProperties { get; set; }

        public MainWindow()
        {
            var referencedAssemblyNamesArray = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            ReferencedAssemblyNames = string.Join("\n"
                , referencedAssemblyNamesArray
                    .Select(ran => ran.ToString())
                    .OrderBy(str => str)
                    .ToArray());

            var assembliesArray = AppDomain.CurrentDomain.GetAssemblies();
            Assemblies = string.Join("\n"
                , assembliesArray
                    .Select(assem => assem.GetName().ToString())
                    .OrderBy(str => str)
                    .ToArray());

            var publicClassesEnumerable = referencedAssemblyNamesArray
                .Select(assemblyName => Assembly.Load(assemblyName))
                .SelectMany(assembly => assembly.GetTypes().Where(type => (type.IsClass && !type.IsAbstract && type.IsPublic)));

            var templateClassesEnumerable = publicClassesEnumerable
                    .Where(type => type.IsSubclassOf(typeof(System.Windows.FrameworkTemplate)));
            TemplateClasses = string.Join("\n"
                , templateClassesEnumerable
                    .Select(type => string.Format("Assembly: {0} {1}, Type: {2}", type.Assembly.GetName().Name, type.Assembly.GetName().Version, type.FullName))
                    .OrderBy(str => str)
                    .ToArray());

            var templatePropertiesEnumerable = publicClassesEnumerable
                .SelectMany(type => type.GetProperties()
                    .Where(pInfo => pInfo.PropertyType.IsSubclassOf(typeof(System.Windows.FrameworkTemplate)))
                    .Select(pInfo => new { Type = type, PropInfo = pInfo }));
            TemplateProperties = string.Join("\n", templatePropertiesEnumerable
                .Select(noName => string.Format("Assembly: {0} {1}, Type: {2}, Property: {3}, Property Type: {4}"
                    , noName.Type.Assembly.GetName().Name, noName.Type.Assembly.GetName().Version, noName.Type.FullName, noName.PropInfo.Name, noName.PropInfo.PropertyType.Name))
                .OrderBy(str => str)
                .ToArray());

            InitializeComponent();
        }
    }
}
