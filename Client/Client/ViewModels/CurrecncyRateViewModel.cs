using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using Client.Commands;
using Client.Models;
using ScottPlot;

namespace Client.ViewModels
{
    public class CurrecncyRateViewModel : INotifyPropertyChanged
    {
        private currencyRate _curRate;
        private List<RateShort> _curList;
        private WpfPlot _wpfPlot;

        private DateTime _startDate;    
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }
        private DateTime _endDate;

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        private int _selectedcCurrecnyIndex;

        public int SelectedcCurrecnyIndex
        {
            get
            {
                return _selectedcCurrecnyIndex;
            }
            set
            {
                _selectedcCurrecnyIndex = value;
                OnPropertyChanged(nameof(SelectedcCurrecnyIndex));
            }
        }

        private BaseCommand _showGraphicsCommand;
        public BaseCommand ShowGraphicsCommand
        {
            get
            {
                return _showGraphicsCommand ??
                    (_showGraphicsCommand = new BaseCommand(async obj =>
                    {
                        DateBuilder dBld = new DateBuilder();

                            if (dBld.ValidateDateTime(_startDate) && dBld.ValidateDateTime(_endDate) && (_endDate >=_startDate))
                            {
                                await _curRate.updateCurrencyRate(_selectedcCurrecnyIndex, _startDate, _endDate);                       

                                if (!_curRate.isRateListEmpty())
                                {

                                    _curList = _curRate.getCurrencyRateList();

                                    _wpfPlot.Plot.Clear();

                                    double[] dataX = new double[_curList.Count];
                                    double[] dataY = new double[_curList.Count];
                                    string[] labels = new string[_curList.Count];

                                    float max = _curList[0].Cur_OfficialRate;
                                    float min = _curList[0].Cur_OfficialRate;

                                    double MinLinePos = 0;
                                    double MaxLinePos = 0;

                                    for (int i = 0; i < _curList.Count; i++)
                                    {
                                        dataY[i] = _curList[i].Cur_OfficialRate;

                                        if(_curList[i].Cur_OfficialRate > max)
                                        {
                                            max = _curList[i].Cur_OfficialRate;

                                            MaxLinePos = i;
                                        }
                                        if(_curList[i].Cur_OfficialRate < min)
                                        {
                                            min = _curList[i].Cur_OfficialRate;

                                            MinLinePos = i;
                                        }
                                    }
                                    for (int i = 0; i < _curList.Count; i++)
                                    {
                                        labels[i] = $"{_curList[i].Date.Day.ToString()}.{_curList[i].Date.Month.ToString()}.{_curList[i].Date.Year.ToString()}";
                                    }
                                    for (int i = 0; i < _curList.Count; i++)
                                    {
                                        dataX[i] = i;
                                    }

                                    _wpfPlot.Plot.AddVerticalLine(MinLinePos, System.Drawing.Color.Red, 1, LineStyle.Dash);
                                    _wpfPlot.Plot.AddVerticalLine(MaxLinePos, System.Drawing.Color.Green, 1, LineStyle.Dash);

                                    _wpfPlot.Plot.AddHorizontalLine(min, System.Drawing.Color.Red, 1, LineStyle.Dash);
                                    _wpfPlot.Plot.AddHorizontalLine(max, System.Drawing.Color.Green, 1, LineStyle.Dash);

                                    _wpfPlot.Plot.XTicks(dataX, labels);
                                    _wpfPlot.Plot.AddScatter(dataX, dataY);
                                    _wpfPlot.Refresh();
                                }
                                else
                                {
                                    MessageBox.Show("Нет данных о валюте");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Неверные даты");
                            }                
                    }));
            }
        }

        public CurrecncyRateViewModel(currencyRate curRate, WpfPlot WpfPlot1)
        {
            this._curRate = curRate;
            this._wpfPlot = WpfPlot1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
