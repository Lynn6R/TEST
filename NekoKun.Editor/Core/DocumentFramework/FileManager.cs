﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NekoKun
{
    public static class FileManager
    {
        private static Dictionary<string, System.WeakReference> dict = new Dictionary<string, System.WeakReference>();

        static FileManager()
        {

        }

        internal static void Open(AbstractFile file)
        {
            System.WeakReference filep = new WeakReference(file);
            dict.Add(file.filename, filep);

            if (dict.Count % 10 == 0)
            {
                Clean();
            }
        }

        private static void Clean()
        {
            Dictionary<string, System.WeakReference> dict2 = new Dictionary<string, System.WeakReference>(dict);
            
            foreach (var item in dict2)
            {
                WeakReference fp = item.Value;
                if (!fp.IsAlive)
                    dict.Remove(item.Key);
            }
        }

        public static AbstractFile Find(string identify)
        {
            WeakReference fp = dict[identify];
            if (fp.IsAlive)
            {
                return dict[identify].Target as AbstractFile;
            }
            else
            {
                dict.Remove(identify);
                throw new ArgumentOutOfRangeException("File disposed.");
            }
        }

        public static void ForEach(Action<AbstractFile> action)
        {
            Clean();
            var list = new List<AbstractFile>();
            var src = new WeakReference[dict.Count];
            dict.Values.CopyTo(src, 0);
            Array.ForEach<WeakReference>(src, delegate(WeakReference fp)
            {
                if (fp.IsAlive)
                    list.Add(fp.Target as AbstractFile);
            });

            Array.ForEach<AbstractFile>(list.ToArray(), action);
        }

        public static NavPoint[] FindAll(string Keyword)
        {
            var result = new List<NavPoint>();
            ForEach(delegate(AbstractFile file) {
                IFindAllProvider findAll = file as IFindAllProvider;

                if (findAll != null)
                    result.AddRange(findAll.FindAll(Keyword));
            });

            return result.ToArray();
        }

        public static event EventHandler PendingChangesStatusChanged;
        private static List<AbstractFile> pendingChanges = new List<AbstractFile>();

        private static void OnPendingChangesStatusChanged()
        {
            if (PendingChangesStatusChanged != null)
                PendingChangesStatusChanged(null, EventArgs.Empty);
        }

        internal static void AddPendingChange(AbstractFile file)
        {
            pendingChanges.Add(file);
            OnPendingChangesStatusChanged();
        }

        internal static void RemovePendingChange(AbstractFile file)
        {
            pendingChanges.Remove(file);
            OnPendingChangesStatusChanged();
        }

        public static void ApplyPendingChanges()
        {
            List<AbstractFile> changes = new List<AbstractFile>(pendingChanges);

            foreach (AbstractFile file in changes)
            {
                if (file.IsDirty)
                    file.Commit();
            }
        }

        public static int PendingChangesCount
        {
            get { return pendingChanges.Count; }
        }

        public static AbstractFile[] PendingChanges
        {
            get
            {
                return pendingChanges.ToArray();
            }
        }
    }
}
