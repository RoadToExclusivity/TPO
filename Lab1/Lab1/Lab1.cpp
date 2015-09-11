#include "stdafx.h"

using namespace std;

struct point
{
	double x = 0.0f, y = 0.0f;
};

vector<string> ParseArgs(const string &args, bool &err)
{
	vector<string> res;
	istringstream iss(args);
	vector<string> points;
	string curBigToken;
	while (getline(iss, curBigToken, ' '))
	{
		istringstream new_iss(curBigToken);
		string curSmallToken;
		int count = 0;
		while (getline(new_iss, curSmallToken, ','))
		{
			count++;
			res.push_back(curSmallToken);
		}
		if (count != 2)
		{
			err = true;
			return res;
		}
	}

	return res;
}

double StrToDouble(const string str, bool &err)
{
	char* lastChar = nullptr;
	double res = strtod(str.c_str(), &lastChar);
	err = ((str.length() == 0) || (*lastChar != '\0'));
	if (errno == ERANGE)
	{
		err = true;
		errno = 0;
	}

	return res;
}

double SideLength(const point &a, const point &b)
{
	return sqrtl((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
}

bool IsEqual(double a, double b)
{
	static const double EPS = 1e-8;

	return (fabsl(a - b) <= EPS);
}

int main(int argc, char* argv[])
{
	string allArgs = "";
	for (size_t i = 1; i < argc; ++i)
	{
		allArgs += argv[i];
		allArgs += " ";
	}
	
	bool err = false;
	
	auto newArgs = ParseArgs(allArgs, err);
	if (err)
	{
		printf_s("Wrong format\n");
		return 0;
	}
	if (newArgs.size() < 6)
	{
		printf_s("Not enough arguments\n");
		return 0;
	}
	if (newArgs.size() > 6)
	{
		printf_s("Too many arguments\n");
		return 0;
	}

	point points[4];
	for (size_t i = 0; i < 3; ++i)
	{
		bool errFirst = false, errSecond = false;
		double x = StrToDouble(newArgs[2 * i], errFirst), y = StrToDouble(newArgs[2 * i + 1], errSecond);
		if (errFirst || errSecond)
		{
			printf_s("Point %u has wrong format\n", i + 1);
			return 0;
		}

		points[i].x = x;
		points[i].y = y;
	}
	points[3] = points[0];

	double lengths[5];
	for (size_t i = 0; i < 3; ++i)
	{
		lengths[i] = SideLength(points[i], points[i + 1]);
	}
	lengths[3] = lengths[0];
	lengths[4] = lengths[1];

	for (size_t i = 0; i < 3; ++i)
	{
		double otherLength = lengths[i + 1] + lengths[i + 2];
		if (IsEqual(lengths[i], 0) || !(!IsEqual(otherLength, lengths[i]) && (lengths[i] < otherLength)))
		{
			printf_s("Not a triangle\n");
			return 0;
		}
	}

	int sum = 0;
	for (size_t i = 0; i < 3; ++i)
	{
		sum += IsEqual(lengths[i], lengths[i + 1]);
	}
	
	switch (sum)
	{
	case 3:
		printf_s("Equilateral triangle\n");
		break;
	case 1:
		printf_s("Isosceles triangle\n");
		break;
	default:
		printf_s("Normal triangle\n");
		break;
	}

	return 0;
}

