/* SimpleDbTests - (C) 2016 - 2019 Premysl Fara 
 
SimpleDbTests is available under the zlib license:

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
 
 */

namespace SimpleDbTests
{
    using System;


    /// <summary>
    /// A test run statistics.
    /// </summary>
    public class TestsRunStatistics
    {
        /// <summary>
        /// The total number of executed tests.
        /// </summary>
        public int ExecutedTestsCount { get; set; }

        /// <summary>
        /// The number of succefully executed tests.
        /// </summary>
        public int SuccededTestsCount { get; set; }

        /// <summary>
        /// The number of failed tests.
        /// </summary>
        public int FailedTestsCount { get; set; }

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return string.Format("Tests executed: {0}, succeded: {1}, failed: {2}", ExecutedTestsCount, SuccededTestsCount, FailedTestsCount);
        }
    }


    /// <summary>
    /// The base class for all tests.
    /// </summary>
    public abstract class ATest
    {
        /// <summary>
        /// Was this test initialized?
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// The statistics of this test.
        /// </summary>
        public TestsRunStatistics Stats { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ATest()
        {
            Initialized = false;
            Stats = new TestsRunStatistics();
        }


        /// <summary>
        /// Should be called before any
        /// </summary>
        public void Initialize()
        {
            if (Initialized) throw new InvalidOperationException("Already initialized.");

            InitializeImplementation();

            Initialized = true;
        }

        /// <summary>
        /// Runs all defined tests.
        /// </summary>
        public abstract void RunAllTests();
        

        #region protected

        /// <summary>
        /// The implementation of the test initialisation.
        /// </summary>
        protected abstract void InitializeImplementation();

        /// <summary>
        /// Checks, if this test was initialized. If not, throws the InvalidOperationException exception.
        /// </summary>
        protected void CheckInitialized()
        {
            if (Initialized == false) throw new InvalidOperationException("Not initialized.");
        }

        /// <summary>
        /// Runs a single test method.
        /// </summary>
        /// <param name="test">A method representing a test step.</param>
        protected void RunTest(Action test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));

            try
            {
                CheckInitialized();

                Console.WriteLine("Executing the {0} test...", test.Method.Name);

                Stats.ExecutedTestsCount++;

                test();

                Stats.SuccededTestsCount++;

                Console.WriteLine("The {0} test finished OK.", test.Method.Name);
            }
            catch (Exception ex)
            {
                Stats.FailedTestsCount++;

                Console.WriteLine("The {0} test FAILED with an exception: {1}", test.Method.Name, ex.Message);
            }

            Console.WriteLine();
        }
        
        #region assertions

        /// <summary>
        /// Checks, if two falues are equal. Throws the AssertionException exception if not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        protected void AssertEqual(int a, int b)
        {
            if (a != b) throw new AssertionException(string.Format("Assertion: {0} is not equal to {1}.", a, b));
        }

        #endregion

        #endregion
    }
}
