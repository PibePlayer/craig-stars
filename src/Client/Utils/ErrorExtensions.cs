using System;
using System.IO;

using Godot;

namespace GodotUtils
{
    /// <summary>
    /// Helper extensions for the Godot.Error class.
    /// From RR|Away on the Godot csharp Discord   
    /// </summary>
    public static class ErrorExtensions
    {
        /// <summary>
        /// Raises an exception if the Error enum was not 'OK'. Otherwise does nothing.
        /// </summary>
        public static void ThrowOnError(this Error error) => ThrowOnError(error, null);

        /// <summary>
        /// Raises an exception if the Error enum was not 'OK'. Otherwise does nothing.
        /// </summary>
        public static void ThrowOnError(this Error error, string message)
        {
            if (error == Error.Ok)
            {
                return;
            }

            if (String.IsNullOrEmpty(message))
            {
                message = "Encountered Godot error.";
            }
            message += $" (Code {(int)error} {Enum.GetName(typeof(Error), error)})";

            // Try to throw a suitable .Net exception matching the Error (might not be exact match)
            switch (error)
            {
                // All errors accounted for up to Godot 3.2.3 (max Error code of 47). Keep errors here
                //	in numerical order to make it easier to keep in sync with Godot.
                case Error.Failed:                      /*   1 */     // Multiple
                case Error.Unavailable:                 /*   2 */    // Multiple
                case Error.Unconfigured:                /*   3 */    // Multiple
                    throw new Exception(message);

                case Error.Unauthorized:                /*   4 */
                    throw new UnauthorizedAccessException(message);

                case Error.ParameterRangeError:         /*   5 */
                    throw new ArgumentOutOfRangeException(null, message);

                case Error.OutOfMemory:                 /*   6 */
                    throw new OutOfMemoryException(message);

                case Error.FileNotFound:                /*   7 */
                    throw new FileNotFoundException(message);
                case Error.FileBadDrive:                /*   8 */
                    throw new DriveNotFoundException(message);
                case Error.FileBadPath:                 /*   9 */
                    throw new FileNotFoundException(message);

                case Error.FileNoPermission:            /*  10 */
                    throw new UnauthorizedAccessException(message);

                case Error.FileAlreadyInUse:            /*  11 */
                case Error.FileCantOpen:                /*  12 */
                case Error.FileCantRead:                /*  13 */
                case Error.FileCantWrite:               /*  14 */
                case Error.FileUnrecognized:            /*  15 */
                case Error.FileCorrupt:                 /*  16 */
                case Error.FileMissingDependencies:     /*  17 */
                case Error.FileEof:                     /*  18 */
                    throw new IOException(message);

                case Error.CantOpen:                    /*  19 */     // Resource / socket
                case Error.CantCreate:                  /*  20 */
                case Error.QueryFailed:                 /*  21 */    // Defined but not in use Godot 3.2.3
                case Error.AlreadyInUse:                /*  22 */
                case Error.Locked:                      /*  23 */
                    throw new IOException(message);

                case Error.Timeout:                     /*  24 */
                    throw new TimeoutException(message);

                case Error.CantConnect:                 /*  25 */
                    throw new IOException(message);

                case Error.CantResolve:                 /*  26 */     // Multiple. Network, and DLL resolution
                    throw new IOException(message);

                case Error.ConnectionError:             /*  27 */
                    throw new IOException(message);

                case Error.CantAcquireResource:         /*  28 */    // Internal engine only @ Godot 3.2.3
                    throw new Exception(message);

                case Error.CantFork:                    /*  29 */    // Internal starting processes
                    throw new Exception(message);

                case Error.InvalidData:                 /*  30 */
                    throw new InvalidDataException(message);

                case Error.InvalidParameter:            /*  31 */
                    throw new ArgumentException(message);

                case Error.AlreadyExists:               /*  32 */    // Multiple. Signals, shaders
                case Error.DoesNotExist:                /*  33 */    // Multiple. Signals, editor
                    throw new Exception(message);

                case Error.DatabaseCantRead:            /*  34 */    // Defined but not in use Godot 3.2.3
                case Error.DatabaseCantWrite:           /*  35 */    // Defined but not in use Godot 3.2.3
                    throw new IOException(message);

                case Error.CompilationFailed:           /*  36 */
                case Error.MethodNotFound:              /*  37 */
                case Error.LinkFailed:                  /*  38 */
                case Error.ScriptFailed:                /*  39 */
                case Error.CyclicLink:                  /*  40 */
                case Error.InvalidDeclaration:          /*  41 */
                case Error.DuplicateSymbol:             /*  42 */
                case Error.ParseError:                  /*  43 */
                    throw new GodotCompilerException(message);

                case Error.Busy:                        /*  44 */    // Multiple
                case Error.Skip:                        /*  45 */    // Multiple
                case Error.Help:                        /*  46 */   // Multiple
                    throw new Exception(message);

                case Error.Bug:                         /*  47 */    // A bug in Godot occured (an assert failed which should always be true!
                    throw new Exception(message);

                default:
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Helper exception for Godot compiler exceptions as there is no comparable .Net or GodotSharp type.
        /// </summary>
        public class GodotCompilerException : Exception
        {
            public GodotCompilerException(string message) : base(message) { }
        }
    }
}