﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using ImGuiNET;
using SoulsFormats;

namespace StudioCore.MsbEditor
{
    class TextEditorScreen : EditorScreen
    {
        public ActionManager EditorActionManager = new ActionManager();

        //private string _activeParam = null;
        //private PARAM.Row _activeRow = null;

        private FMGBank.ItemCategory[] _displayCategories =
        {
            FMGBank.ItemCategory.Armor,
            FMGBank.ItemCategory.Characters,
            FMGBank.ItemCategory.Goods,
            FMGBank.ItemCategory.Locations,
            FMGBank.ItemCategory.Rings,
            FMGBank.ItemCategory.Spells,
            FMGBank.ItemCategory.Weapons
        };

        private FMGBank.ItemCategory _activeCategory = FMGBank.ItemCategory.None;
        private string _activeCategoryDS2 = null;
        private List<FMG.Entry> _cachedEntries = null;
        private FMG.Entry _activeEntry = null;

        private FMG.Entry _cachedTitle = null;
        private FMG.Entry _cachedSummary = null;
        private FMG.Entry _cachedDescription = null;

        //private PropertyEditor _propEditor = null;

        public TextEditorScreen(Sdl2Window window, GraphicsDevice device)
        {
            //_propEditor = new PropertyEditor(EditorActionManager);
        }

        public override void DrawEditorMenu()
        {
            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "CTRL+Z", false, EditorActionManager.CanUndo()))
                {
                    EditorActionManager.UndoAction();
                }
                if (ImGui.MenuItem("Redo", "Ctrl+Y", false, EditorActionManager.CanRedo()))
                {
                    EditorActionManager.RedoAction();
                }
                if (ImGui.MenuItem("Delete", "Delete", false, Selection.IsSelection()))
                {
                }
                if (ImGui.MenuItem("Duplicate", "Ctrl+D", false, Selection.IsSelection()))
                {
                }
                ImGui.EndMenu();
            }
        }

        private void EditorGUI(bool doFocus)
        {
            ImGui.Columns(3);
            ImGui.BeginChild("categories");
            ImGui.AlignTextToFramePadding();
            foreach (var cat in _displayCategories)
            {
                if (ImGui.Selectable(cat.ToString(), cat == _activeCategory))
                {
                    _activeCategory = cat;
                    _cachedEntries = FMGBank.GetEntriesOfCategoryAndType(cat, FMGBank.ItemType.Title);
                }
                if (doFocus && cat == _activeCategory)
                {
                    ImGui.SetScrollHereY();
                }
            }
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("rows");
            if (_activeCategory == FMGBank.ItemCategory.None)
            {
                ImGui.Text("Select a category to see items");
            }
            else
            {
                foreach (var r in _cachedEntries)
                {
                    var text = (r.Text == null) ? "%null%" : r.Text;
                    if (ImGui.Selectable($@"{r.ID} {text}", _activeEntry == r))
                    {
                        _activeEntry = r;
                        FMGBank.LookupItemID(r.ID, _activeCategory, out _cachedTitle, out _cachedSummary, out _cachedDescription);
                    }
                    if (doFocus && _activeEntry == r)
                    {
                        ImGui.SetScrollHereY();
                    }
                }
            }
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("text");
            if (_activeEntry == null)
            {
                ImGui.Text("Select an item to edit text");
            }
            else
            {
                //_propEditor.PropEditorParamRow(_activeRow);
                ImGui.Columns(2);
                ImGui.Text("ID");
                ImGui.NextColumn();
                int id = _activeEntry.ID;
                ImGui.InputInt("##id", ref id);
                ImGui.NextColumn();

                if (_cachedTitle != null)
                {
                    ImGui.Text("Title");
                    ImGui.NextColumn();
                    string text = (_cachedTitle.Text != null) ? _cachedTitle.Text : "";
                    ImGui.InputText("##title", ref text, 255);
                    ImGui.NextColumn();
                }

                if (_cachedSummary != null)
                {
                    ImGui.Text("Summary");
                    ImGui.NextColumn();
                    string text = (_cachedSummary.Text != null) ? _cachedSummary.Text : "";
                    ImGui.InputTextMultiline("##summary", ref text, 1000, new Vector2(-1, 80.0f));
                    ImGui.NextColumn();
                }

                if (_cachedDescription != null)
                {
                    ImGui.Text("Description");
                    ImGui.NextColumn();
                    string text = (_cachedDescription.Text != null) ? _cachedDescription.Text : "";
                    ImGui.InputTextMultiline("##description", ref text, 1000, new Vector2(-1, 160.0f));
                    ImGui.NextColumn();
                }
            }
            ImGui.EndChild();
        }

        private void EditorGUIDS2(bool doFocus)
        {
            if (FMGBank.DS2Fmgs == null)
            {
                return;
            }

            ImGui.Columns(3);
            ImGui.BeginChild("categories");
            foreach (var cat in FMGBank.DS2Fmgs.Keys)
            {
                if (ImGui.Selectable(cat, cat == _activeCategoryDS2))
                {
                    _activeCategoryDS2 = cat;
                    _cachedEntries = FMGBank.DS2Fmgs[cat].Entries;
                }
                if (doFocus && cat == _activeCategoryDS2)
                {
                    ImGui.SetScrollHereY();
                }
            }
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("rows");
            if (_activeCategoryDS2 == null)
            {
                ImGui.Text("Select a category to see items");
            }
            else
            {
                foreach (var r in _cachedEntries)
                {
                    var text = (r.Text == null) ? "%null%" : r.Text;
                    if (ImGui.Selectable($@"{r.ID} {text}", _activeEntry == r))
                    {
                        _activeEntry = r;
                    }
                    if (doFocus && _activeEntry == r)
                    {
                        ImGui.SetScrollHereY();
                    }
                }
            }
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("text");
            if (_activeEntry == null)
            {
                ImGui.Text("Select an item to edit text");
            }
            else
            {
                //_propEditor.PropEditorParamRow(_activeRow);
                ImGui.Columns(2);
                ImGui.Text("ID");
                ImGui.NextColumn();
                int id = _activeEntry.ID;
                ImGui.InputInt("##id", ref id);
                ImGui.NextColumn();


                ImGui.Text("Text");
                ImGui.NextColumn();
                string text = (_activeEntry.Text != null) ? _activeEntry.Text : "";
                ImGui.InputTextMultiline("##description", ref text, 1000, new Vector2(-1, 160.0f));
                ImGui.NextColumn();
            }
            ImGui.EndChild();
        }

        public void OnGUI(string[] initcmd)
        {
            if (FMGBank.AssetLocator == null)
            {
                return;
            }

            // Docking setup
            //var vp = ImGui.GetMainViewport();
            var wins = ImGui.GetWindowSize();
            var winp = ImGui.GetWindowPos();
            winp.Y += 20.0f;
            wins.Y -= 20.0f;
            ImGui.SetNextWindowPos(winp);
            ImGui.SetNextWindowSize(wins);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0.0f);
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            flags |= ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
            flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
            flags |= ImGuiWindowFlags.NoBackground;
            //ImGui.Begin("DockSpace_MapEdit", flags);
            ImGui.PopStyleVar(4);
            //var dsid = ImGui.GetID("DockSpace_ParamEdit");
            //ImGui.DockSpace(dsid, new Vector2(0, 0));

            // Keyboard shortcuts
            if (EditorActionManager.CanUndo() && InputTracker.GetControlShortcut(Key.Z))
            {
                EditorActionManager.UndoAction();
            }
            if (EditorActionManager.CanRedo() && InputTracker.GetControlShortcut(Key.Y))
            {
                EditorActionManager.RedoAction();
            }

            bool doFocus = false;
            // Parse select commands
            if (initcmd != null && initcmd[0] == "select")
            {
                if (initcmd.Length > 1)
                {
                    doFocus = true;
                    foreach (var cat in _displayCategories)
                    {
                        if (cat.ToString() == initcmd[1])
                        {
                            _activeCategory = cat;
                            _cachedEntries = FMGBank.GetEntriesOfCategoryAndType(cat, FMGBank.ItemType.Title);
                            break;
                        }
                    }
                    if (initcmd.Length > 2)
                    {
                        int id;
                        var parsed = int.TryParse(initcmd[2], out id);
                        if (parsed)
                        {
                            var r = _cachedEntries.FirstOrDefault(r => r.ID == id);
                            if (r != null)
                            {
                                _activeEntry = r;
                                FMGBank.LookupItemID(r.ID, _activeCategory, out _cachedTitle, out _cachedSummary, out _cachedDescription);
                            }
                        }
                    }
                }
            }

            if (FMGBank.AssetLocator.Type == GameType.DarkSoulsIISOTFS)
            {
                EditorGUIDS2(doFocus);
            }
            else
            {
                EditorGUI(doFocus);
            }
        }

        public override void OnProjectChanged(ProjectSettings newSettings)
        {
        }

        public override void Save()
        {
            throw new NotImplementedException();
        }

        public override void SaveAll()
        {
            throw new NotImplementedException();
        }
    }
}
