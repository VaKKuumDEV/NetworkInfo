<?php
class Point{
	private $x;
	private $y;
	
	public function __construct(float $x, float $y){
		$this->x = $x;
		$this->y = $y;
	}
	
	public function getX(): float{
		return $this->x;
	}
	
	public function getY(): float{
		return $this->y;
	}
}

class Distance{
	private $point;
	private $distance;
	
	public function __construct(Point $point, int $distance){
		$this->point = $point;
		$this->distance = $distance;
	}
	
	public function getPoint(): Point{
		return $this->point;
	}
	
	public function getDistance(): int{
		return $this->distance;
	}
}

class Database{
	private $db;
	
	public function __construct(){
		mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
		$this->db = new mysqli("localhost", "admin", "password", "app");
		$this->db->set_charset('utf8mb4');
		
		if ($this->db->connect_errno) {
			throw new \RuntimeException('Ошибка соединения mysqli: ' . $this->db->connect_error);
		}
	}
	
	public function insert(string $table, array $data): bool{
		$keys = [];
		$values = [];
		
		foreach($data as $key => $value){
			$keys[] = '`' . $key . '`';
			if(is_numeric($value)) $values[] = $value;
			else if(is_null($value)) $values[] = 'NULL';
			else if($value instanceof Point) $values[] = 'ST_PointFromText(\'POINT(' . $value->getX() . ' ' . $value->getY() . ')\')';
			else $values[] = "'" . $this->real_string($value) . "'";
		}
		
		$sql = "INSERT INTO `" . $table . "`(" . implode(", ", $keys) . ") VALUES (" . implode(", ", $values) . ")";
		$result = $this->db->query($sql);
		return $result;
	}
	
	public function updateData(string $table, array $data, array $where): bool{
		$keys = [];
		$values = [];
		
		$joinedData = [];
		$joinedWhere = [];
		
		foreach($data as $key => $value){
			if(is_string($value)) $value = "'" . $this->real_string($value) . "'";
			else if(is_null($value)) $value = 'NULL';
			
			$joinedData[] = $key . ' = ' . $value;
		}
		
		$sql = "UPDATE " . $table . " SET " . implode(', ', $joinedData);
		
		if(count($where) > 0){
			$w = [];
			foreach($where as $key => $value){
				if(is_null($value)) $w[] = $key . ' IS NULL';
				else{
					if(is_array($value)){
						$whereVals = [];
						foreach($value as $subVal){
							if(is_null($subVal)) $subVal = ' IS NULL';
							else if(!is_numeric($value)) $subVal = '\'' . $this->real_string($subVal) . '\'';
							
							$whereVals[] = $key . ' = ' . $subVal;
						}
						
						$w[] = '(' . implode(' OR ', $whereVals) . ')';
					}else{
						if(!is_numeric($value)) $value = "'" . $this->real_string($value) . "'";
						$w[] = $key . " = " . $value;
					}
				}
			}
			
			$sql .= " WHERE " . implode(" AND ", $w);
		}
		
		$result = $this->db->query($sql);
		return $result;
	}
	
	public function query(string $sql): ?array{
		$result = $this->db->query($sql);
		if($result->num_rows == 1){
			$rows = $result->fetch_assoc();
			return $rows;
		}else if($result->num_rows > 1){
			$rows = [];
			while ($row = $result->fetch_assoc()) {
				$rows[] = $row;
			}
			return $rows;
		}
		
		return null;
	}
	
	public function update(string $sql): bool{
		$result = $this->db->query($sql);
		return $result;
	}
	
	public function real_string(string $str): string{
		return $this->db->real_escape_string($str);
	}
	
	public function getQuery($table, array $where = [], array $orders = [], ?int $limit = null, array $joins = [], ?string $distinctColumn = null): ?array{
		$sql = "SELECT";
		
		if($distinctColumn != null) $sql .= ' DISTINCT(' . $distinctColumn . ')';
		else $sql .= ' *';
		$sql .= ' FROM ' . $table;
		
		if(count($joins) > 0){
			$w = [];
			foreach($joins as $key => $value){
				$w[] = 'LEFT JOIN ' . $key . ' ON ' . $value;
			}
			
			$sql .= ' ' . implode(' ', $w);
		}
		
		if(count($where) > 0){
			$w = [];
			foreach($where as $key => $value){
				if(is_null($value)) $w[] = $key . ' IS NULL';
				else{
					if($value instanceof Distance){
						$radius = $value->getDistance() / 111000;
						
						$fromLat = $value->getPoint()->getX() - $radius;
						$fromLong = $value->getPoint()->getY() - $radius;
						$toLat = $value->getPoint()->getX() + $radius;
						$toLong = $value->getPoint()->getY() + $radius;
						
						$w[] = 'X(' . $key . ') BETWEEN ' . $fromLat . ' AND ' . $toLat . ' AND Y(' . $key . ') BETWEEN ' . $fromLong . ' AND ' . $toLong;
					}
					else if(is_array($value)){
						$whereVals = [];
						foreach($value as $subVal){
							if(is_null($subVal)) $subVal = ' IS NULL';
							else if(!is_numeric($value)) $subVal = '\'' . $this->real_string($subVal) . '\'';
							
							$whereVals[] = $key . ' = ' . $subVal;
						}
						
						$w[] = '(' . implode(' OR ', $whereVals) . ')';
					}else{
						if(!is_numeric($value)) $value = "'" . $this->real_string($value) . "'";
						$w[] = $key . " = " . $value;
					}
				}
			}
			
			$sql .= " WHERE " . implode(" AND ", $w);
		}
		
		if(count($orders) > 0){
			$o = [];
			foreach($orders as $key => $value){
				$order = "DESC";
				if(strtolower($value) == "asc") $order = "ASC";
				$o[] = $key . " " . $order;
			}
			
			$sql .= " ORDER BY " . implode(" AND ", $o);
		}
		
		if(is_numeric($limit)) $sql .= " LIMIT " . $limit;
		
		$result = $this->query($sql);
		return $result;
	}
}
?>