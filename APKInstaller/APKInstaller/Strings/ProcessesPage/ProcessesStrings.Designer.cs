﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace APKInstaller.Strings.ProcessesPage {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ProcessesStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ProcessesStrings() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("APKInstaller.Strings.ProcessesPage.ProcessesStrings", typeof(ProcessesStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Detail 的本地化字符串。
        /// </summary>
        public static string Detail_Header {
            get {
                return ResourceManager.GetString("Detail.Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Waiting for Device... 的本地化字符串。
        /// </summary>
        public static string DeviceComboBox_PlaceholderText {
            get {
                return ResourceManager.GetString("DeviceComboBox.PlaceholderText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Name 的本地化字符串。
        /// </summary>
        public static string Name_Header {
            get {
                return ResourceManager.GetString("Name.Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Process ID 的本地化字符串。
        /// </summary>
        public static string ProcessId_Header {
            get {
                return ResourceManager.GetString("ProcessId.Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Resident Set Size 的本地化字符串。
        /// </summary>
        public static string ResidentSetSize_Header {
            get {
                return ResourceManager.GetString("ResidentSetSize.Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 State 的本地化字符串。
        /// </summary>
        public static string State_Header {
            get {
                return ResourceManager.GetString("State.Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Processes 的本地化字符串。
        /// </summary>
        public static string TitleBar_Title {
            get {
                return ResourceManager.GetString("TitleBar.Title", resourceCulture);
            }
        }
    }
}
