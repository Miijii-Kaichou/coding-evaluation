using System.Text;
using System.Collections.Immutable;
using System.Reflection.PortableExecutable;

namespace MyOrganization
{
    internal abstract class Organization
    {
        private readonly Position root;

        private StringBuilder? stringBuilder;

        private readonly HashSet<Employee> EmployeeList;
        private const int EmployeeIdentifierBase = 2023000;

        public Organization()
        {
            EmployeeList = new HashSet<Employee>();
            root = CreateOrganization();
        }

        protected abstract Position CreateOrganization();

        /**
         * hire the given person as an employee in the position that has that title
         * 
         * @param person
         * @param title
         * @return the newly filled position or empty if no position has that title
         */
        public Position? Hire(Name person, string title)
        {
            Position? newPosition = new(title);
            stringBuilder = new();

            CheckPositionTitleStatus(ref newPosition, out (bool status, string reason) result);

            if (result.status == false)
            {
                Console.WriteLine(result.reason);
                return null;
            }

            int employeeID = EmployeeIdentifierBase + EmployeeList.Count;
            Employee? newEmployee = new(employeeID, person);

            newPosition.SetEmployee(newEmployee);

            EmployeeList.Add(newEmployee);

            return newPosition;
        }

        private void CheckPositionTitleStatus(ref Position newPosition, out (bool status, string reason) result)
        {
            bool isPositionOpen = true;
            string title = newPosition.GetTitle();

            result = (isPositionOpen, "Undefined");

            StringBuilder? sb = stringBuilder;
            ImmutableList<Position> directReports = root.GetDirectReports();

            bool doesPositionExist = FindPositionOfTitle(title, directReports, out Position? position);

            sb?.Append("There is no position with the title \"");
            sb?.Append(title);
            sb?.Append('\"');

            if (doesPositionExist == false)
            {
                isPositionOpen = doesPositionExist;
                result = (isPositionOpen, sb?.ToString()!);
                return;
            }

            sb?.Clear();

            ValidatePositionOccupancy(directReports, title, out bool isTaken);

            sb?.Append("Thank you for you interest at American Airlines.");
            sb?.Append("It seems this position with the title \"");
            sb?.Append(title);
            sb?.Append("\" Is already filled.");

            if (isTaken)
            {
                isPositionOpen = !isTaken;
                result = (isPositionOpen, sb?.ToString()!);
                return;
            }

            sb?.Clear();
            newPosition = position!;
        }

        private bool FindPositionOfTitle(string title, ImmutableList<Position> directReports, out Position? result)
        {
            result = null;

            Position? position = result;
            bool foundPosition = false;
            int i = 0;

            if (root.GetTitle().Equals(title))
            {
                foundPosition = true;
                position = root;
            }

            while(foundPosition == false && i < directReports.Count)
            {
                position = directReports[i];
                ImmutableList<Position> subDirectReports = position.GetDirectReports();
                
                foundPosition = position.GetTitle().Equals(title);

                if (foundPosition) continue; 

                if (subDirectReports.Count != 0)
                {
                    foundPosition = FindPositionOfTitle(title, subDirectReports, out position);
                    i++;
                    continue;
                }

                i++;
            }

            result = position;
            return foundPosition;
        }

        private static void ValidatePositionOccupancy(ImmutableList<Position> directReports, string title, out bool result)
        {
            result = false;

            Position? position = directReports?.Find(p => p.GetTitle() == title);
            if (position == null) return;

            result = position.IsFilled();
        }

        override public string ToString()
        {
            return PrintOrganization(root, "");
        }

        private string PrintOrganization(Position pos, string prefix)
        {
            StringBuilder sb = new(prefix + "+-" + pos.ToString() + "\n");
            foreach (Position p in pos.GetDirectReports())
            {
                sb.Append(PrintOrganization(p, prefix + "  "));
            }
            return sb.ToString();
        }
    }
}
