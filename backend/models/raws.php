<?php
class RawsModel{
	private static $instance;
	private $db;
	
	public static function newInstance(){
		if(!self::$instance instanceof self){
			self::$instance = new self;
		}
		return self::$instance;
	}
	
	public function __construct(){
		$this->db = new Database();
	}
	
	public function getById(int $id): array{
		$row = $this->db->getQuery('raws', ['row_id' => $id]);
		if($row == null) throw new \Exception("Указанная запись не найдена");
		
		return $row;
	}
	
	public function getRow(int $rowId): array{
		$row = $this->getById($rowId);
		
		$newRow = $this->loadRow($row);
		return $newRow;
	}
	
	public function loadRow(array $data): array{
		$coordinates = unpack('x/x/x/x/corder/Ltype/dlon/dlat', $data['row_point']);
		
		$newRow = [
			'id' => intval($data['row_id']),
			'creation_date' => $data['row_creation_date'],
			'long' => $coordinates['lat'],
			'lat' => $coordinates['lon'],
			'operator' => $data['row_operator'],
			'speed' => intval($data['row_speed']),
			'strength' => floatval($data['row_strength']),
			'device' => $data['row_device'],
			'type' => intval($data['row_type']),
		];
		
		return $newRow;
	}
	
	private function haversineGreatCircleDistance($latitudeFrom, $longitudeFrom, $latitudeTo, $longitudeTo, $earthRadius = 6371000){
		$latFrom = deg2rad($latitudeFrom);
		$lonFrom = deg2rad($longitudeFrom);
		$latTo = deg2rad($latitudeTo);
		$lonTo = deg2rad($longitudeTo);
		$lonDelta = $lonTo - $lonFrom;
		$a = pow(cos($latTo) * sin($lonDelta), 2) + pow(cos($latFrom) * sin($latTo) - sin($latFrom) * cos($latTo) * cos($lonDelta), 2);
		$b = sin($latFrom) * sin($latTo) + cos($latFrom) * cos($latTo) * cos($lonDelta);
		$angle = atan2(sqrt($a), $b);
		return $angle * $earthRadius;
	}
	
	private function averagePoints(array $points, array $notViewed, array $seen = [], int $averageRadius = 50, int $type): array{
		$resultPoints = [];
		while(count($notViewed) > 0){
			if(in_array($notViewed[0]['id'], $seen)) continue;
			
			$strengthSum = 0;
			$speedSum = 0;
			$count = 0;
			
			$seenCurrent = [];
			for($j = 0; $j < count($points); $j++){
				$distance = $this->haversineGreatCircleDistance($notViewed[0]['lat'], $notViewed[0]['long'], $points[$j]['lat'], $points[$j]['long']);
				if($distance <= $averageRadius){
					$count++;
					$strengthSum += $points[$j]['strength'];
					$speedSum += $points[$j]['speed'];
					$seenCurrent[] = $points[$j]['id'];
				}
			}
			
			$averagedPoint = [
				'id' => $notViewed[0]['id'],
				'lat' => $notViewed[0]['lat'],
				'long' => $notViewed[0]['long'],
				'operator' => $notViewed[0]['operator'],
				'speed' => $speedSum / $count,
				'strength' => round($strengthSum / $count),
				'radius' => $averageRadius,
				'device' => "",
				'type' => $type,
			];
			$resultPoints[] = $averagedPoint;
			
			$newNotViewed = [];
			for($i = 0; $i < count($notViewed); $i++) if(!in_array($notViewed[$i]['id'], $seenCurrent)) $newNotViewed[] = $notViewed[$i];
			$notViewed = $newNotViewed;
			$seen = array_merge($seen, $seenCurrent);
		}
		
		return $resultPoints;
	}
	
	public function getOperators(): array{
		$operators = $this->db->getQuery('raws', [], [], null, [], 'row_operator');
		
		if($operators == null) $operators = [];
		else if((count($operators) - count($operators, COUNT_RECURSIVE)) == 0) $operators = [$operators];
		
		$newOperators = [];
		foreach($operators as $operator) $newOperators[] = $operator['row_operator'];
		
		return $newOperators;
	}
	
	public function getRaws(string $operator, float $lat, float $long, int $radiusMetres, int $type): array{
		$where = ['row_operator' => $operator, 'row_point' => new Distance(new Point($lat, $long), 2 * $radiusMetres), 'row_type' => $type];
		
		$raws = $this->db->getQuery('raws', $where, ['row_creation_date' => 'desc'], 1000);
		if($raws == null) $raws = [];
		else if((count($raws) - count($raws, COUNT_RECURSIVE)) == 0) $raws = [$raws];
		
		$newRaws = [];
		foreach($raws as $raw){
			$newRaw = $this->loadRow($raw);
			$newRaws[] = $newRaw;
		}
		
		$averagedPoints = $this->averagePoints($newRaws, $newRaws, [], round(0.2 * $radiusMetres), $type);
		
		return $averagedPoints;
	}
	
	public function sendRaw(string $operator, float $lat, float $long, float $speed, int $strength, string $device, int $type): void{
		$insertData = [
			'row_point' => new Point($lat, $long),
			'row_operator' => $operator,
			'row_device' => $device,
			'row_speed' => $speed,
			'row_strength' => $strength,
			'row_type' => $type,
		];
		
		$this->db->insert('raws', $insertData);
	}
}
?>