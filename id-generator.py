import pandas as pd

pd.read_csv('vgchartz.csv', header = None).to_csv('file2.csv', header = False)