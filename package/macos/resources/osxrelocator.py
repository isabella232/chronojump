#!/usr/bin/env python3
# cerbero - a multi-platform build system for Open Source software
# Copyright (C) 2012 Andoni Morales Alastruey <ylatuya@gmail.com>
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Library General Public
# License as published by the Free Software Foundation; either
# version 2 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Library General Public License for more details.
#
# You should have received a copy of the GNU Library General Public
# License along with this library; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA.

import os
import subprocess


INT_CMD = 'install_name_tool'
OTOOL_CMD = 'otool'


class OSXRelocator(object):
    '''
    Wrapper for OS X's install_name_tool and otool commands to help
    relocating shared libraries.

    It parses lib/ /libexec and bin/ directories, changes the prefix path of
    the shared libraries that an object file uses and changes it's library
    ID if the file is a shared library.
    '''

    def __init__(self, root, lib_prefix, recursive, logfile=None):
        self.root = root
        self.lib_prefix = self._fix_path(lib_prefix)
        self.recursive = recursive
        self.use_relative_paths = True
        self.logfile = None

    def relocate(self):
        self.parse_dir(self.root)

    def relocate_dir(self, dirname):
        self.parse_dir(os.path.join(self.root, dirname))

    def relocate_file(self, object_file):
        self.change_libs_path(object_file)

    def change_id(self, object_file, id=None):
        id = id or object_file.replace(self.lib_prefix, '@rpath')
        filename = os.path.basename(object_file)
        if not (filename.endswith('so') or filename.endswith('dylib')):
            return
        subprocess.call([INT_CMD, "-id", id, object_file])

    def change_libs_path(self, object_file):
        depth = len(object_file.split('/')) - len(self.root.split('/')) - 1
        p_depth = '/..' * depth
        rpaths = ['.']
        rpaths += ['@loader_path' + p_depth, '@executable_path' + p_depth]
        rpaths += ['@loader_path' + '/../lib', '@executable_path' + '/../lib']
        if not (object_file.endswith('so') or object_file.endswith('dylib')):
            return
        if depth > 1:
            rpaths += ['@loader_path/..', '@executable_path/..']
        for p in rpaths:
            subprocess.call([INT_CMD, "-add_rpath", p, object_file])
        for lib in self.list_shared_libraries(object_file):
            if self.lib_prefix in lib:
                new_lib = lib.replace(self.lib_prefix, '@rpath')
                subprocess.call([INT_CMD, '-change', lib, new_lib, object_file])

    def change_lib_path(self, object_file, old_path, new_path):
        for lib in self.list_shared_libraries(object_file):
            if old_path in lib:
                new_path = lib.replace(old_path, new_path)
                cmd = '%s -change "%s" "%s" "%s"' % (INT_CMD, lib, new_path,
                                               object_file)
                subprocess.call(cmd, fail=True, logfile=self.logfile)

    def parse_dir(self, dir_path, filters=None):
        for dirpath, dirnames, filenames in os.walk(dir_path):
            for f in filenames:
                if filters is not None and \
                        os.path.splitext(f)[1] not in filters:
                    continue
                self.change_libs_path(os.path.join(dirpath, f))
            if not self.recursive:
                break

    @staticmethod
    def list_shared_libraries(object_file):
        res = subprocess.getoutput("{} -L {}".format(OTOOL_CMD, object_file)).splitlines()
        # We don't use the first line
        libs = res[1:]
        # Remove the first character tabulation
        libs = [str(x[1:]) for x in libs]
        # Remove the version info
        libs = [x.split(' ', 1)[0] for x in libs]
        return libs

    def _fix_path(self, path):
        if path.endswith('/'):
            return path[:-1]
        return path


class Main(object):

    def run(self):
        # We use OptionParser instead of ArgumentsParse because this script
        # might be run in OS X 10.6 or older, which do not provide the argparse
        # module
        import optparse
        usage = "usage: %prog [options] library_path old_prefix new_prefix"
        description = 'Rellocates object files changing the dependant '\
                      ' dynamic libraries location path with a new one'
        parser = optparse.OptionParser(usage=usage, description=description)
        parser.add_option('-r', '--recursive', action='store_true',
                default=False, dest='recursive',
                help='Scan directories recursively')

        options, args = parser.parse_args()
        if len(args) != 3:
            parser.print_usage()
            exit(1)
        relocator = OSXRelocator(args[1], args[2], options.recursive)
        relocator.relocate_dir(args[0])
        exit(0)


if __name__ == "__main__":
    main = Main()
    main.run()
