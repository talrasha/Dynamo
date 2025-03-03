﻿
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    public class PackagePathTests : SystemTestBase
    {
        #region PackagePathViewModelTests
        [Test]
        public void CannotDeletePathIfThereIsOnlyOne()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(1, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(null));
        }

        [Test]
        public void ReorderingPathsTest()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"C:\", @"D:\", @"E:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(0));
            Assert.IsFalse(vm.MovePathUpCommand.CanExecute(0));

            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(2));
            Assert.IsFalse(vm.MovePathDownCommand.CanExecute(2));

            Assert.IsTrue(vm.MovePathUpCommand.CanExecute(1));
            Assert.IsTrue(vm.MovePathDownCommand.CanExecute(1));

            vm.MovePathUpCommand.Execute(1);

            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"E:\", vm.RootLocations[2]);

            vm.MovePathDownCommand.Execute(1);

            Assert.AreEqual(@"D:\", vm.RootLocations[0]);
            Assert.AreEqual(@"E:\", vm.RootLocations[1]);
            Assert.AreEqual(@"C:\", vm.RootLocations[2]);
        }

        [Test]
        public void CannotDeleteStandardLibraryPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"%StandardLibrary%", @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(0));
            Assert.IsTrue(vm.DeletePathCommand.CanExecute(1));
        }

        [Test]
        public void CannotUpdateStandardLibraryPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { @"%StandardLibrary%", @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.UpdatePathCommand.CanExecute(0));
            Assert.IsTrue(vm.UpdatePathCommand.CanExecute(1));
        }
        [Test]
        public void CannotDeleteProgramDataPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { Path.Combine(ViewModel.Model.PathManager.CommonDataDirectory,"Packages"), @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.DeletePathCommand.CanExecute(0));
            Assert.IsTrue(vm.DeletePathCommand.CanExecute(1));
        }

        [Test]
        public void CannotUpdateProgramDataPath()
        {
            var setting = new PreferenceSettings
            {
                CustomPackageFolders = { Path.Combine(ViewModel.Model.PathManager.CommonDataDirectory, "Packages"), @"C:\" }
            };


            var vm = CreatePackagePathViewModel(setting);

            Assert.AreEqual(2, vm.RootLocations.Count);
            Assert.IsFalse(vm.UpdatePathCommand.CanExecute(0));
            Assert.IsTrue(vm.UpdatePathCommand.CanExecute(1));
        }

        [Test]
        public void AddRemovePathsTest()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = @"C:\";
            vm.AddPathCommand.Execute(null);
            path = @"D:\";
            vm.AddPathCommand.Execute(null);

            Assert.AreEqual(@"C:\", vm.RootLocations[1]);
            Assert.AreEqual(@"D:\", vm.RootLocations[2]);

            vm.DeletePathCommand.Execute(0);

            Assert.AreEqual(@"C:\", vm.RootLocations[0]);
            Assert.AreEqual(@"D:\", vm.RootLocations[1]);
        }
        [Test]
        public void AddPackagePathsTest()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);

            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            var testDir = GetTestDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            path = Path.Combine(testDir, @"core\packagePathTest");
            var dynFilePath = Path.Combine(path, @"dynFile\Number1.dyn");
            vm.AddPathCommand.Execute(null);
            vm.SaveSettingCommand.Execute(null);
            Model.ExecuteCommand(new DynamoModel.OpenFileCommand(dynFilePath));
            Assert.AreEqual(1,GetPreviewValue("07d62dd8-b2f3-40a8-a761-013d93300444"));
        }

        [Test]
        public void PathEnabledConverterCustomPaths()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);
            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = @"C:\";
            vm.AddPathCommand.Execute(null);
            var x = new PathEnabledConverter();
            Assert.False((bool)x.Convert(new object[] { vm, path },null,null,null ));

            setting.DisableCustomPackageLocations = true;

            Assert.True((bool)x.Convert(new object[] { vm, path }, null, null, null));
        }

        [Test]
        public void PathEnabledConverterStdLibPath()
        {
            var setting = new PreferenceSettings()
            {
                CustomPackageFolders = { @"Z:\" }
            };

            var vm = CreatePackagePathViewModel(setting);
            var path = string.Empty;
            vm.RequestShowFileDialog += (sender, args) => { args.Path = path; };

            path = "Standard Library";
            vm.AddPathCommand.Execute(null);
            var x = new PathEnabledConverter();
            Assert.False((bool)x.Convert(new object[] { vm, path }, null, null, null));

            setting.DisableStandardLibrary = true;

            Assert.True((bool)x.Convert(new object[] { vm, path }, null, null, null));
            Assert.False((bool)x.Convert(new object[] { vm, @"Z:\" }, null, null, null));
        }

       

        #endregion
        #region Setup methods
        private PackagePathViewModel CreatePackagePathViewModel(PreferenceSettings setting)
        {
            PackageLoader loader = new PackageLoader(setting.CustomPackageFolders);
            LoadPackageParams loadParams = new LoadPackageParams
            {
                Preferences = setting,
                PathManager = Model.PathManager
            };
            CustomNodeManager customNodeManager = Model.CustomNodeManager;
            return new PackagePathViewModel(loader, loadParams, customNodeManager);
        }
        #endregion
    }

    class PackagePathTests_CustomPrefs : DynamoTestUIBase
    {
        /// <summary>
        /// Derived test classes can override this method to provide different configurations.
        /// </summary>
        /// <param name="pathResolver">A path resolver to pass to the DynamoModel. </param>
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings()
                {
                    //program data first
                    CustomPackageFolders = { Path.Combine(GetCommonDataDirectory(),PathManager.PackagesDirectoryName),  @"C:\", GetAppDataFolder(), }
                }
            };
        }

        [Test]
        [Category("TechDebt")]
        public void IfProgramDataPathIsFirstDefaultPackagePathIsStillAppData()
        {
            var setting = Model.PreferenceSettings;
            var loader = Model.GetPackageManagerExtension().PackageLoader;

            var appDataFolder = Path.Combine(GetAppDataFolder());
            Assert.AreEqual(3, ViewModel.Model.PathManager.PackagesDirectories.Count());
            Assert.AreEqual(4, setting.CustomPackageFolders.Count);
            Assert.AreEqual(Path.Combine(appDataFolder, PathManager.PackagesDirectoryName), ViewModel.Model.PathManager.DefaultPackagesDirectory);
            Assert.AreEqual(appDataFolder, setting.SelectedPackagePathForInstall);
            //TODO this is incorrect, but this property has been obsoleted and will be made correct after refactoring pathmanager and packageloader.
            //this should be changed to Assert.AreEqual after refactor.
            Assert.AreNotEqual(Path.Combine(appDataFolder, PathManager.PackagesDirectoryName), loader.DefaultPackagesDirectory);
        }
    }
}
