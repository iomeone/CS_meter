import os

if __name__ == '__main__':
    '''
    This script merges the rows of each csv file in the 'individual-matches'
    directory into a single csv file, 'all_data.csv'.

    This script operates on files which are located relative to itself. It
    reads from an input directory, 'individual-matches' which is a sibling of
    the script. It writes to an output file, all_data.csv which is also a
    sibling of the script.

    The input csv files are generated from src/parse-demos
    '''

    # This script works with files which are located relative to itself
    cur_dir = os.path.abspath(os.path.dirname(__file__))
    
    # Directory to our individual match csv files. Should be a sibling of this python script.
    read_dir = os.path.join(cur_dir, 'individual-matches')

    output_file = os.path.join(cur_dir, 'all_data.csv')

    # Set to false after the first file so we copy only the header once
    first_file = True

    with open(output_file, 'w') as output_fp:
        for file in os.listdir(read_dir):

            full_file_path = os.path.join(cur_dir, read_dir, file)

            # Set to false after the first line of each file so we only copy the header once
            first_line = True

            with open(full_file_path) as input_fp:
                for line in input_fp:
                    
                    if first_line:

                        if first_file:
                            first_file = False
                            output_fp.write(line)

                        first_line = False
                        continue
                    
                    output_fp.write(line)
                    


