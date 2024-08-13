/*
 * Ventuz.ImageSharp.Native
 * Copyright (c) 2024 Ventuz Technology <https://ventuz.com>
 *
 * This file is part of Ventuz.ImageSharp.Native
 *
 * Ventuz.ImageSharp.Native is free software: you can redistribute
 * it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later
 * version.
 *
 * Ventuz.ImageSharp.Native is distributed in the hope that it will
 * be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Ventuz.ImageSharp.Native.  If not, see <http://www.gnu.org/licenses/>.
 */

// dllmain.cpp : Defines the entry point for the DLL application.
#include "windows.h"

BOOL APIENTRY DllMain(HMODULE hModule,
                      DWORD  ul_reason_for_call,
                      LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

