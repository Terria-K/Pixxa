using System;
using System.IO;
using ImGuiNET;

namespace Pixxa;

public static class FileDialog 
{
    
    public static void OpenFileDialog(string id, string filter, object obj, ref bool condition, ref string path) 
    {
        if (ImGui.BeginPopupModal(id, ref condition, ImGuiWindowFlags.NoTitleBar)) 
        {
            var picker = FilePicker.GetFilePicker(obj, Path.Combine(Environment.CurrentDirectory), filter);
            if (picker.Draw()) 
            {
                path = picker.SelectedFile;
                FilePicker.RemoveFilePicker(obj);
            }
            ImGui.EndPopup();
        }
    }

    public static void OpenFolderDialog(string id, object obj, ref bool condition, ref string path) 
    {
        if (ImGui.BeginPopupModal(id, ref condition, ImGuiWindowFlags.NoTitleBar)) 
        {
            var picker = FilePicker.GetFolderPicker(obj, Path.Combine(Environment.CurrentDirectory));
            if (picker.Draw()) 
            {
                path = picker.SelectedFile;
                FilePicker.RemoveFilePicker(obj);
            }
            ImGui.EndPopup();
        }
    }
}